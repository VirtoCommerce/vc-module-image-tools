<template>
  <VcPopup
    :title="title"
    variant="warning"
    @close="$emit('close')"
  >
    <template #content>
      <div class="choice-popup">
        <p>{{ message }}</p>
        <ul v-if="notes?.length">
          <li
            v-for="note in notes"
            :key="note"
          >
            {{ note }}
          </li>
        </ul>
      </div>
    </template>
    <template #footer="{ close }">
      <!-- the framework's confirmation dialog layout: text-styled actions, filled Cancel -->
      <div class="vc-popup-base__actions vc-popup-base__actions--confirm">
        <VcButton
          v-for="choice in choices"
          :key="choice.value"
          text
          class="vc-popup-base__confirm"
          :disabled="choice.disabled"
          @click="$emit('choose', choice.value)"
        >
          {{ choice.label }}
        </VcButton>
        <VcButton @click="close">{{ cancelLabel }}</VcButton>
      </div>
    </template>
  </VcPopup>
</template>

<script setup lang="ts">
// A confirmation with more than one way to proceed (the AngularJS run/reindex dialogs)
import { VcButton, VcPopup } from "@vc-shell/framework/ui";

defineProps<{
  title: string;
  message: string;
  notes?: string[];
  choices: { value: string; label: string; disabled?: boolean }[];
  cancelLabel: string;
}>();
defineEmits<{ (e: "close"): void; (e: "choose", value: string): void }>();
</script>

<style lang="scss">
.choice-popup {
  font-size: 14px;
  color: var(--neutrals-700);
  p {
    margin: 0 0 8px;
  }
  ul {
    margin: 0;
    padding-left: 20px;
    list-style: disc;
  }
}
</style>
