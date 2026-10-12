const tooltipDuration = 1300;
const copiedDuration = 1400;

export class CopyToClipboardButton {
    readonly #element: HTMLElement;
    readonly #tooltip: HTMLElement | undefined;
    readonly #status: HTMLElement | undefined;
    readonly #copiedText: string;

    #tooltipTimeout: ReturnType<typeof setTimeout> | undefined;
    #copiedTimeout: ReturnType<typeof setTimeout> | undefined;

    constructor(element: HTMLElement, copiedText: string) {
        this.#element = element;
        this.#tooltip = element.querySelector<HTMLElement>(':scope > .tooltip') ?? undefined;
        this.#status = element.querySelector<HTMLElement>(':scope > .status') ?? undefined;
        this.#copiedText = copiedText;
    }

    async #tryCopyTextToClipboard(text: string): Promise<boolean> {
        // The Clipboard API only exists in secure contexts, which a page served over plain HTTP is not.
        if (globalThis.isSecureContext) {
            const isWritten = await navigator.clipboard.writeText(text)
                .then(() => true)
                .catch(() => false);

            if (isWritten)
                return true;
        }

        const focusedElement = document.activeElement;
        const textArea = document.createElement('textarea');

        textArea.value = text;
        textArea.readOnly = true;
        textArea.style.position = 'fixed';
        textArea.style.top = '-1000px';
        textArea.style.opacity = '0';

        document.body.append(textArea);

        try {
            textArea.select();

            // Deprecated, but the only way to copy where the Clipboard API is unavailable.
            // eslint-disable-next-line @typescript-eslint/no-deprecated
            return document.execCommand('copy');
        } catch {
            return false;
        } finally {
            textArea.remove();

            if (focusedElement instanceof HTMLElement)
                focusedElement.focus();
        }
    }

    #showCopiedState() {
        this.#clearTimeouts();

        this.#element.classList.add('copy-to-clipboard-button--copied');
        this.#tooltip?.classList.add('tooltip--visible');

        if (this.#status)
            this.#status.textContent = this.#copiedText;

        this.#tooltipTimeout = setTimeout(() => {
            this.#tooltip?.classList.remove('tooltip--visible');
        }, tooltipDuration);

        this.#copiedTimeout = setTimeout(() => {
            this.#hideCopiedState();
        }, copiedDuration);
    }

    #hideCopiedState() {
        this.#element.classList.remove('copy-to-clipboard-button--copied');

        if (this.#status)
            this.#status.textContent = '';
    }

    #clearTimeouts() {
        clearTimeout(this.#tooltipTimeout);
        clearTimeout(this.#copiedTimeout);
    }

    /**
     Copies the given text to the clipboard and confirms it when it succeeded
     */
    async copy(text: string) {
        if (await this.#tryCopyTextToClipboard(text))
            this.#showCopiedState();
    }

    dispose() {
        this.#clearTimeouts();
    }
}
