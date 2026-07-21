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
        return Number.parseFloat(this.#element.style.left) || 0;
    }

    public get top(): number {
        return Number.parseFloat(this.#element.style.top) || 0;
    }

    public setContent(content: HTMLElement) {
        this.#element.replaceChildren(content);
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
