<template>
  <VcBlade
    :title="job?.title ?? $t('THUMBNAILS.PAGES.PROGRESS.TITLE')"
    :toolbar-items="bladeToolbar"
    width="50%"
  >
    <VcContainer>
      <JobProgress
        :job="job"
        :finished="finished"
        :percent="percent"
        prefix="THUMBNAILS"
      />
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useApiClient, useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { ThumbnailsTasksClient, type ThumbnailProcessNotification } from "../../api_client/virtocommerce.imagetools";
import JobProgress from "../components/JobProgress.vue";
import { useJob, type JobNotification } from "../composables/useJob";
import { errorText } from "../utils";

defineBlade({
  url: "/thumbnail-progress",
  name: "ThumbnailsTaskProgress",
});

const { t } = useI18n({ useScope: "global" });
const { options, callParent } = useBlade<{ notification: ThumbnailProcessNotification }>();
const { showError } = usePopup();
const { getApiClient } = useApiClient(ThumbnailsTasksClient);
const { job, finished, percent, follow } = useJob();

if (options.value?.notification) follow(options.value.notification as JobNotification);

const cancelUnsupported = ref(false);

// the task's "last run" changes when the job ends
watch(finished, (done) => done && callParent("reload"));

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "cancel",
    title: t("THUMBNAILS.PAGES.PROGRESS.TOOLBAR.CANCEL"),
    icon: "lucide-circle-x",
    disabled: computed(() => finished.value || cancelUnsupported.value || !job.value?.jobId),
    async clickHandler() {
      try {
        const client = await getApiClient();
        await client.cancel(job.value!.jobId!);
      } catch (e) {
        // 501: the background job engine can't recall a running job
        if ((e as { status?: number }).status === 501) {
          cancelUnsupported.value = true;
          showError(t("THUMBNAILS.ALERTS.CANCEL_UNSUPPORTED"));
        } else showError(errorText(e));
      }
    },
  },
]);
</script>
