export class DragGhostHost {
    readonly #element: HTMLElement;

    constructor() {
        const element = document.createElement('div');
        element.style.position = 'absolute';
        element.style.userSelect = 'none';
        element.style.opacity = '0.3';

        this.#element = element;
    }

    public get element(): HTMLElement {
        return this.#element;
    }

    public get offsetWidth(): number {
        return this.#element.offsetWidth;
    }

    public get offsetHeight(): number {
        return this.#element.offsetHeight;
    }

    public get left(): number {
        const left = Number.parseFloat(this.#element.style.left);
        return Number.isNaN(left) ? 0 : left;
    }

    public get top(): number {
        const top = Number.parseFloat(this.#element.style.top);
        return Number.isNaN(top) ? 0 : top;
    }

    public setContent(content: HTMLElement) {
        this.#element.replaceChildren(content);
    }

    public setCursor(cursor: string | undefined) {
        this.#element.style.cursor = cursor ?? '';
    }

    public appendTo(parent: HTMLElement) {
        parent.append(this.#element);
    }

    public setPosition(left: number, top: number) {
        this.#element.style.left = `${left}px`;
        this.#element.style.top = `${top}px`;
    }

    public remove() {
        this.#element.remove();
    }
}
