import { reactive } from "vue";
import { usePopup } from "@vc-shell/framework";
import ChoicePopup from "../components/ChoicePopup.vue";

type Choice = { value: string; label: string; disabled?: boolean };
type Ask = { title: string; message: string; notes?: string[]; choices: Choice[]; cancelLabel: string };

// Opens ChoicePopup; resolves with the chosen value, undefined on cancel. Call during setup.
export function useChoice() {
  const props = reactive<Ask>({ title: "", message: "", notes: [], choices: [], cancelLabel: "" });
  let resolver: ((value?: string) => void) | undefined;
  const { open, close } = usePopup({
    component: ChoicePopup,
    props,
    emits: {
      onClose() {
        resolver?.();
        close();
      },
      onChoose(value: string) {
        resolver?.(value);
        close();
      },
    },
  });

  function ask(options: Ask): Promise<string | undefined> {
    Object.assign(props, options);
    return new Promise((resolve) => {
      resolver = (value) => {
        resolver = undefined;
        resolve(value);
      };
      open();
    });
  }

  return { ask };
}
