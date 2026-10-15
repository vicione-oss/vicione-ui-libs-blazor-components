export class TextBox {
    readonly #input: HTMLInputElement | undefined;
    #revertValue: string;

    readonly #inputKeyDown = (event: KeyboardEvent) => {
        if (event.code === 'Escape' && this.#input?.value !== this.#revertValue)
            event.stopPropagation();
    };

    /**
     Blazor passes null for an input that left the page before the instance was created; then there is nothing to listen to
     */
    constructor(input: HTMLInputElement | undefined, revertValue: string | undefined) {
        this.#input = input ?? undefined;
        this.#revertValue = revertValue ?? '';

        this.#input?.addEventListener('keydown', this.#inputKeyDown);
    }

    /**
     Sets the value Escape reverts the input to
     */
    setRevertValue(revertValue: string | undefined) {
        this.#revertValue = revertValue ?? '';
    }

    /**
     Selects the content of the given input
     */
    selectContent(input: HTMLInputElement) {
        input.select();
    }

    dispose() {
        this.#input?.removeEventListener('keydown', this.#inputKeyDown);
    }
}
