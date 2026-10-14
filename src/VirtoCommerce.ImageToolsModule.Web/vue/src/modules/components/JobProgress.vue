<template>
  <div class="job-progress tw-space-y-4">
    <VcCard :header="job?.title">
      <div class="job-progress__body">
        <div class="job-progress__state">
          <VcIcon
            :icon="stateIcon"
            :class="['job-progress__icon', `job-progress__icon--${state}`]"
          />
          <span>{{ job?.description }}</span>
        </div>
        <VcProgress
          v-if="!finished"
          :value="percent"
          :variant="job?.totalCount ? 'default' : 'striped'"
        />
        <dl class="job-progress__facts">
          <dt>{{ $t(`${prefix}.JOB.STARTED`) }}</dt>
          <dd>{{ formatTime(job?.created) }}</dd>
          <template v-if="finished">
            <dt>{{ $t(`${prefix}.JOB.FINISHED`) }}</dt>
            <dd>{{ formatTime(job?.finished) }}</dd>
          </template>
          <dt>{{ $t(`${prefix}.JOB.ELAPSED`) }}</dt>
          <dd>{{ elapsed }}</dd>
          <template v-if="job?.totalCount">
            <dt>{{ $t(`${prefix}.JOB.PROCESSED`) }}</dt>
            <dd>{{ job.processedCount ?? 0 }} / {{ job.totalCount }}</dd>
          </template>
          <dt>{{ $t(`${prefix}.JOB.ERRORS`) }}</dt>
          <dd>{{ job?.errorCount ?? job?.errors?.length ?? 0 }}</dd>
        </dl>
        <slot />
      </div>
    </VcCard>
    <VcCard
      v-if="job?.errors?.length"
      :header="$t(`${prefix}.JOB.ERRORS`)"
    >
      <div class="job-progress__log">
        <p
          v-for="(error, i) in job.errors"
          :key="i"
        >
          {{ error }}
        </p>
      </div>
    </VcCard>
  </div>
</template>

<script setup lang="ts">
// Progress of a background job, rendered from its push notification
import { VcCard, VcIcon, VcProgress } from "@vc-shell/framework/ui";
import { computed, onBeforeUnmount, ref, watch } from "vue";
import type { JobNotification } from "../composables/useJob";

const props = defineProps<{ job?: JobNotification; finished: boolean; percent: number; prefix: string }>();

const now = ref(Date.now());
const tick = setInterval(() => (now.value = Date.now()), 1000);
watch(
  () => props.finished,
  (done) => done && clearInterval(tick),
);
onBeforeUnmount(() => clearInterval(tick));

const hasErrors = computed(() => (props.job?.errorCount ?? props.job?.errors?.length ?? 0) > 0);
const state = computed(() => (!props.finished ? "running" : hasErrors.value ? "error" : "done"));
const stateIcon = computed(
  () => ({ running: "lucide-loader-circle", error: "lucide-circle-alert", done: "lucide-circle-check" })[state.value],
);

const formatTime = (value?: Date | string | null) => (value ? new Date(value).toLocaleString() : "—");

const elapsed = computed(() => {
  if (!props.job?.created) return "—";
  const end = props.job.finished ? new Date(props.job.finished).getTime() : now.value;
  const s = Math.max(0, Math.floor((end - new Date(props.job.created).getTime()) / 1000));
  return [Math.floor(s / 3600), Math.floor((s % 3600) / 60), s % 60].map((x) => String(x).padStart(2, "0")).join(":");
});
</script>

<style lang="scss">
.job-progress {
  padding: 20px 22px 36px;
  &__body {
    display: flex;
    flex-direction: column;
    gap: 16px;
    padding: 16px;
    font-size: 14px;
    color: var(--neutrals-700);
  }
  &__state {
    display: flex;
    align-items: center;
    gap: 8px;
    color: var(--neutrals-900);
  }
  &__icon {
    &--running {
      color: var(--primary-500);
      animation: job-progress-spin 1s linear infinite;
    }
    &--done {
      color: var(--success-500);
    }
    &--error {
      color: var(--danger-500);
    }
  }
  &__facts {
    display: grid;
    grid-template-columns: max-content 1fr;
    gap: 8px 24px;
    margin: 0;
    dt {
      color: var(--neutrals-500);
    }
    dd {
      margin: 0;
      font-variant-numeric: tabular-nums;
    }
  }
  &__log {
    max-height: 320px;
    overflow: auto;
    padding: 12px 16px;
    font-family: ui-monospace, monospace;
    font-size: 12px;
    color: var(--danger-600);
    p {
      margin: 0 0 6px;
    }
  }
}
@keyframes job-progress-spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
