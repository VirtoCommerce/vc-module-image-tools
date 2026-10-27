<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="thumbnails-form tw-space-y-4">
        <VcInput
          v-model="item.name"
          :label="$t('THUMBNAILS.PAGES.TASK.FIELDS.NAME')"
          :placeholder="$t('THUMBNAILS.PAGES.TASK.FIELDS.NAME_HINT')"
          required
        />
        <VcInput
          v-model="item.workPath"
          :label="$t('THUMBNAILS.PAGES.TASK.FIELDS.PATH')"
          :hint="$t('THUMBNAILS.PAGES.TASK.FIELDS.PATH_HINT')"
          required
        />
        <VcSelect
          v-model="optionIds"
          :label="$t('THUMBNAILS.PAGES.TASK.FIELDS.OPTIONS')"
          :hint="$t('THUMBNAILS.PAGES.TASK.FIELDS.OPTIONS_HINT')"
          :options="allOptions"
          option-value="id"
          option-label="name"
          multiple
          searchable
          required
        />
        <VcField
          v-if="item.id"
          :label="$t('THUMBNAILS.PAGES.TASK.FIELDS.LAST_RUN')"
          :model-value="item.lastRun"
          type="date"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useApiClient,
  useAsync,
  useBlade,
  useBladeForm,
  useBladeWidgets,
  useLoading,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcContainer, VcField, VcForm, VcInput, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import {
  ThumbnailsOptionsClient,
  ThumbnailsTasksClient,
  type ThumbnailOption,
  type ThumbnailTask,
} from "../../api_client/virtocommerce.imagetools";
import { errorText } from "../utils";

defineBlade({
  url: "/thumbnail-task",
  name: "ThumbnailsTaskDetails",
});

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, closeSelf, exposeToChildren } = useBlade();
const { showConfirmation, showError } = usePopup();
const { getApiClient } = useApiClient(ThumbnailsTasksClient);
const { getApiClient: getOptionsClient } = useApiClient(ThumbnailsOptionsClient);

const item = ref<ThumbnailTask>({ thumbnailOptions: [] });
const allOptions = ref<ThumbnailOption[]>([]);
const isNew = computed(() => !item.value.id);
const title = computed(() => (isNew.value ? t("THUMBNAILS.PAGES.TASK.NEW") : (item.value.name ?? t("THUMBNAILS.PAGES.TASK.TITLE"))));

const optionIds = computed<string[]>({
  get: () => (item.value.thumbnailOptions ?? []).map((x) => x.id!),
  set: (ids) => (item.value.thumbnailOptions = ids.map((id) => allOptions.value.find((o) => o.id === id)!).filter(Boolean)),
});

const valid = computed(() => !!item.value.name?.trim() && !!item.value.workPath?.trim() && optionIds.value.length > 0);
const form = useBladeForm({
  data: item,
  canSaveOverride: valid,
  closeConfirmMessage: computed(() => t("THUMBNAILS.ALERTS.CLOSE_CONFIRMATION")),
});

const { loading: itemLoading, action: loadItem } = useAsync<string>(async (id) => {
  const client = await getApiClient();
  item.value = await client.getThumbnailTask(id!);
});

async function loadOptions() {
  const client = await getOptionsClient();
  allOptions.value = (await client.searchThumbnailOption({ skip: 0, take: 1000 })).results ?? [];
}

// the Save button shows its own error popup
const { loading: saveLoading, action: save } = useAsync(
  async () => {
    const client = await getApiClient();
    if (isNew.value) {
      item.value = await client.createThumbnailTask(item.value);
    } else {
      await client.updateThumbnailTask(item.value);
      item.value = await client.getThumbnailTask(item.value.id!);
    }
  },
  { notify: false },
);

const loading = useLoading(itemLoading, saveLoading);

async function remove() {
  if (!(await showConfirmation(t("THUMBNAILS.ALERTS.DELETE_TASKS", { count: 1 })))) return;
  try {
    const client = await getApiClient();
    await client.deleteThumbnailTask([item.value.id!]);
    form.setBaseline();
    await callParent("reload");
    await closeSelf();
  } catch (e) {
    showError(errorText(e));
  }
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("THUMBNAILS.PAGES.TASK.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: ["thumbnail:update", "thumbnail:create"],
    disabled: computed(() => !form.canSave.value),
    async clickHandler() {
      try {
        await save();
        form.setBaseline();
        await callParent("reload");
      } catch (e) {
        showError(errorText(e));
      }
    },
  },
  {
    id: "reset",
    title: t("THUMBNAILS.PAGES.TASK.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "run",
    title: t("THUMBNAILS.PAGES.TASK.TOOLBAR.RUN"),
    icon: "lucide-play",
    disabled: computed(() => isNew.value || form.isModified.value),
    // the progress opens next to the list, as the AngularJS blade did
    clickHandler: () => callParent("runTasks", [item.value]),
  },
  {
    id: "delete",
    title: t("THUMBNAILS.PAGES.TASK.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "thumbnail:delete",
    disabled: isNew,
    clickHandler: () => remove(),
  },
]);

useBladeWidgets([
  {
    id: "ThumbnailOptions",
    icon: "lucide-sliders-horizontal",
    title: "THUMBNAILS.PAGES.TASK.WIDGETS.OPTIONS",
    badge: computed(() => allOptions.value.length),
    onClick: () => openBlade({ name: "ThumbnailsOptionList" }),
  },
]);

onMounted(async () => {
  await Promise.all([loadOptions(), param.value ? loadItem(param.value) : Promise.resolve()]);
  form.setBaseline();
});

// the settings blades below change the option list this task picks from
exposeToChildren({ reloadOptions: loadOptions });
</script>

<style lang="scss">
.thumbnails-form {
  padding: 20px 22px 36px;
}
</style>
