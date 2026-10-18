<template>
  <VcBlade
    :title="$t('THUMBNAILS.PAGES.OPTIONS.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      :items="items"
      state-key="THUMBNAIL_OPTIONS"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :empty-state="{ icon: 'lucide-sliders-horizontal', title: $t('THUMBNAILS.PAGES.OPTIONS.EMPTY') }"
      @row-click="(e: { data: ThumbnailOption }) => openOption(e.data.id)"
    >
      <VcColumn
        id="name"
        :title="$t('THUMBNAILS.PAGES.OPTIONS.COLUMNS.NAME')"
        body-class="thumbnails-list__name"
        always-visible
      />
      <VcColumn
        id="resizeMethod"
        :title="$t('THUMBNAILS.PAGES.OPTIONS.COLUMNS.METHOD')"
      >
        <template #body="{ data: option }">{{ option.resizeMethod ? $t(`THUMBNAILS.RESIZE.${option.resizeMethod}`) : "" }}</template>
      </VcColumn>
      <VcColumn
        id="size"
        :title="$t('THUMBNAILS.PAGES.OPTIONS.COLUMNS.SIZE')"
        always-visible
      >
        <template #body="{ data: option }">{{ option.width }} × {{ option.height }}</template>
      </VcColumn>
      <VcColumn
        id="fileSuffix"
        :title="$t('THUMBNAILS.PAGES.OPTIONS.COLUMNS.SUFFIX')"
      />
      <VcColumn
        id="backgroundColor"
        :title="$t('THUMBNAILS.PAGES.OPTIONS.COLUMNS.BACKGROUND')"
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useApiClient, useAsync, useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { ThumbnailsOptionsClient, type ThumbnailOption } from "../../api_client/virtocommerce.imagetools";

defineBlade({
  url: "/thumbnail-options",
  name: "ThumbnailsOptionList",
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, callParent, exposeToChildren } = useBlade();
const { getApiClient } = useApiClient(ThumbnailsOptionsClient);

const items = ref<ThumbnailOption[]>([]);
const activeItemId = ref<string>();

const { loading, action: load } = useAsync(async () => {
  const client = await getApiClient();
  items.value = (await client.searchThumbnailOption({ sort: "name", skip: 0, take: 1000 })).results ?? [];
});

function openOption(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: "ThumbnailsOptionDetails",
    param: id,
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

load();

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("THUMBNAILS.PAGES.OPTIONS.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  {
    id: "add",
    title: t("THUMBNAILS.PAGES.OPTIONS.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "thumbnail:create",
    clickHandler: () => openOption(),
  },
]);

exposeToChildren({
  async reload(id?: string) {
    await load();
    if (id) activeItemId.value = id;
    // the task above picks from this list
    await callParent("reloadOptions");
  },
});
</script>
