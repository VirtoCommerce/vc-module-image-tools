<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="thumbnails-form tw-space-y-4">
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.name"
              :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.NAME')"
              required
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.fileSuffix"
              :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.SUFFIX')"
              :hint="$t('THUMBNAILS.PAGES.OPTION.FIELDS.SUFFIX_HINT')"
              required
            />
          </VcCol>
        </VcRow>
        <VcSelect
          v-model="item.resizeMethod"
          :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.METHOD')"
          :options="resizeOptions"
          :clearable="false"
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.width"
              type="number"
              :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.WIDTH')"
              required
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.height"
              type="number"
              :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.HEIGHT')"
              required
            />
          </VcCol>
        </VcRow>
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.backgroundColor"
              :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.BACKGROUND')"
              :hint="$t('THUMBNAILS.PAGES.OPTION.FIELDS.BACKGROUND_HINT')"
            />
          </VcCol>
          <VcCol>
            <VcSelect
              v-model="item.anchorPosition"
              :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.ANCHOR')"
              :options="anchorOptions"
              :clearable="false"
            />
          </VcCol>
        </VcRow>
        <VcSelect
          v-model="item.jpegQuality"
          :label="$t('THUMBNAILS.PAGES.OPTION.FIELDS.QUALITY')"
          :options="qualityOptions"
          :clearable="false"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useApiClient, useAsync, useBlade, useBladeForm, useLoading, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcCol, VcContainer, VcForm, VcInput, VcRow, VcSelect } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import {
  AnchorPosition,
  JpegQuality,
  ResizeMethod,
  ThumbnailsOptionsClient,
  type ThumbnailOption,
} from "../../api_client/virtocommerce.imagetools";
import { errorText } from "../utils";

defineBlade({
  url: "/thumbnail-option",
  name: "ThumbnailsOptionDetails",
});

const { t } = useI18n({ useScope: "global" });
const { param, callParent, closeSelf } = useBlade();
const { showConfirmation, showError } = usePopup();
const { getApiClient } = useApiClient(ThumbnailsOptionsClient);

// defaults of a new setting, as in the AngularJS blade
const item = ref<ThumbnailOption>({
  resizeMethod: ResizeMethod.FixedSize,
  anchorPosition: AnchorPosition.Center,
  jpegQuality: JpegQuality.High,
});
const isNew = computed(() => !item.value.id);
const title = computed(() => (isNew.value ? t("THUMBNAILS.PAGES.OPTION.NEW") : (item.value.name ?? t("THUMBNAILS.PAGES.OPTION.TITLE"))));

// computed: a new options array on every render makes a select rebuild its option cache
const enumOptions = (values: Record<string, string>, key: string) =>
  computed(() => Object.values(values).map((id) => ({ id, title: t(`THUMBNAILS.${key}.${id}`) })));
const resizeOptions = enumOptions(ResizeMethod, "RESIZE");
const anchorOptions = enumOptions(AnchorPosition, "ANCHOR");
const qualityOptions = enumOptions(JpegQuality, "QUALITY");

const positive = (v?: number | string) => /^\d+$/.test(String(v)) && Number(v) > 0;
const valid = computed(() => !!item.value.name?.trim() && !!item.value.fileSuffix?.trim() && positive(item.value.width) && positive(item.value.height));
const form = useBladeForm({
  data: item,
  canSaveOverride: valid,
  closeConfirmMessage: computed(() => t("THUMBNAILS.ALERTS.CLOSE_CONFIRMATION")),
});

const { loading: itemLoading, action: loadItem } = useAsync<string>(async (id) => {
  const client = await getApiClient();
  item.value = await client.getThumbnailOption(id!);
});

// the Save button shows its own error popup
const { loading: saveLoading, action: save } = useAsync(
  async () => {
    const client = await getApiClient();
    const body = { ...item.value, width: Number(item.value.width), height: Number(item.value.height) };
    if (isNew.value) {
      item.value = await client.createThumbnailOption(body);
    } else {
      await client.updateThumbnailOption(body);
      item.value = await client.getThumbnailOption(item.value.id!);
    }
  },
  { notify: false },
);

const loading = useLoading(itemLoading, saveLoading);

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("THUMBNAILS.PAGES.OPTION.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: ["thumbnail:update", "thumbnail:create"],
    disabled: computed(() => !form.canSave.value),
    async clickHandler() {
      try {
        await save();
        form.setBaseline();
        await callParent("reload", item.value.id);
      } catch (e) {
        showError(errorText(e));
      }
    },
  },
  {
    id: "reset",
    title: t("THUMBNAILS.PAGES.OPTION.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "delete",
    title: t("THUMBNAILS.PAGES.OPTION.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "thumbnail:delete",
    disabled: isNew,
    async clickHandler() {
      if (!(await showConfirmation(t("THUMBNAILS.ALERTS.DELETE_OPTION")))) return;
      try {
        const client = await getApiClient();
        await client.deleteThumbnailOption([item.value.id!]);
        form.setBaseline();
        await callParent("reload");
        await closeSelf();
      } catch (e) {
        showError(errorText(e));
      }
    },
  },
]);

onMounted(async () => {
  if (param.value) await loadItem(param.value);
  form.setBaseline();
});
</script>
