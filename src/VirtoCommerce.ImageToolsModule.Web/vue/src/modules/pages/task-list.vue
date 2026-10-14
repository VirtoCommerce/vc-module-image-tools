<template>
  <VcBlade
    :title="$t('THUMBNAILS.MENU.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      v-model:selection="selection"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      state-key="THUMBNAIL_TASKS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :row-actions="rowActions"
      :empty-state="{ icon: 'lucide-images', title: $t('THUMBNAILS.PAGES.TASKS.EMPTY') }"
      @row-click="(e: { data: ThumbnailTask }) => openTask(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('THUMBNAILS.PAGES.TASKS.COLUMNS.NAME')"
        body-class="thumbnails-list__name"
        sortable
        always-visible
      />
      <VcColumn
        id="workPath"
        :title="$t('THUMBNAILS.PAGES.TASKS.COLUMNS.PATH')"
        sortable
      />
      <VcColumn
        id="thumbnailOptions"
        :title="$t('THUMBNAILS.PAGES.TASKS.COLUMNS.OPTIONS')"
      >
        <template #body="{ data: task }">{{ sizes(task) }}</template>
      </VcColumn>
      <VcColumn
        id="lastRun"
        :title="$t('THUMBNAILS.PAGES.TASKS.COLUMNS.LAST_RUN')"
        type="date-ago"
        sortable
        always-visible
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useApiClient,
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  usePermissions,
  usePopup,
  type IBladeToolbar,
  type TableAction,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { ThumbnailsTasksClient, type ThumbnailTask } from "../../api_client/virtocommerce.imagetools";
import { useChoice } from "../composables/useChoice";
import { errorText } from "../utils";

defineBlade({
  url: "/thumbnails",
  name: "ThumbnailsTaskList",
  isWorkspace: true,
  menuItem: {
    title: "THUMBNAILS.MENU.TITLE",
    icon: "lucide-images",
    priority: 60,
  },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, closeChildren, exposeToChildren } = useBlade();
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();
const { ask } = useChoice();
const { getApiClient } = useApiClient(ThumbnailsTasksClient);

const PAGE_SIZE = 50;
const items = ref<ThumbnailTask[]>([]);
const totalCount = ref(0);
const selection = ref<ThumbnailTask[]>([]);
const activeItemId = ref<string>();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "THUMBNAIL_TASKS", initialField: "name", initialDirection: "ASC" });
const pagination = useDataTablePagination({ stateKey: "THUMBNAIL_TASKS", pageSize: PAGE_SIZE, totalCount });

const { loading, action: load } = useAsync(async () => {
  const client = await getApiClient();
  const result = await client.searchThumbnailTask({ sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE });
  items.value = result.results ?? [];
  totalCount.value = result.totalCount ?? 0;
});

// "(168x168, 216x216, 64x64)" like the AngularJS list
const sizes = (task: ThumbnailTask) =>
  (task.thumbnailOptions ?? [])
    .slice(0, 3)
    .map((o) => `${o.width}x${o.height}`)
    .join(", ") + ((task.thumbnailOptions?.length ?? 0) > 3 ? ", …" : "");

function openTask(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: "ThumbnailsTaskDetails",
    param: id,
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

async function runTasks(tasks: ThumbnailTask[]) {
  const choice = await ask({
    title: t("THUMBNAILS.RUN.TITLE"),
    message: t("THUMBNAILS.RUN.MESSAGE"),
    notes: [t("THUMBNAILS.RUN.NOTE_CHANGES"), t("THUMBNAILS.RUN.NOTE_REGENERATE")],
    choices: [
      { value: "changes", label: t("THUMBNAILS.RUN.CHANGES"), disabled: tasks.some((x) => !x.lastRun) },
      { value: "regenerate", label: t("THUMBNAILS.RUN.REGENERATE") },
    ],
    cancelLabel: t("THUMBNAILS.RUN.CANCEL"),
  });
  if (!choice) return;
  try {
    const client = await getApiClient();
    const notification = await client.run({ taskIds: tasks.map((x) => x.id!), regenerate: choice === "regenerate" });
    await openBlade({ name: "ThumbnailsTaskProgress", options: { notification } });
  } catch (e) {
    showError(errorText(e));
  }
}

async function deleteTasks(tasks: ThumbnailTask[]) {
  if (!(await showConfirmation(t("THUMBNAILS.ALERTS.DELETE_TASKS", { count: tasks.length })))) return;
  await closeChildren();
  try {
    const client = await getApiClient();
    await client.deleteThumbnailTask(tasks.map((x) => x.id!));
    selection.value = [];
  } catch (e) {
    showError(errorText(e));
  }
  await load();
}

const rowActions = (task: ThumbnailTask): TableAction[] => [
  { id: "run", icon: "lucide-play", title: t("THUMBNAILS.PAGES.TASKS.TOOLBAR.RUN"), type: "info", clickHandler: () => runTasks([task]) },
  {
    id: "delete",
    icon: "lucide-trash-2",
    title: t("THUMBNAILS.PAGES.TASKS.TOOLBAR.DELETE"),
    type: "danger",
    variant: "danger",
    hidden: !hasAccess("thumbnail:delete"),
    clickHandler: () => deleteTasks([task]),
  },
];

load();
watch([sortExpression, () => pagination.skip], () => load());

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("THUMBNAILS.PAGES.TASKS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("THUMBNAILS.PAGES.TASKS.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "thumbnail:create",
    clickHandler: () => openTask(),
  },
  {
    id: "run",
    title: t("THUMBNAILS.PAGES.TASKS.TOOLBAR.RUN"),
    icon: "lucide-play",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => runTasks(selection.value),
  },
  {
    id: "delete",
    title: t("THUMBNAILS.PAGES.TASKS.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "thumbnail:delete",
    disabled: computed(() => !selection.value.length),
    clickHandler: () => deleteTasks(selection.value),
  },
]);

exposeToChildren({ reload: () => load(), runTasks });
</script>

<style lang="scss">
.thumbnails-list__name {
  color: var(--neutrals-900);
}
</style>
