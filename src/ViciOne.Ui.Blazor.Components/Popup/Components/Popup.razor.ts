export class Popup {
    readonly #dotNetObject: DotNet.DotNetObject;
    readonly #modalDialogElement: HTMLElement;

    // Keys are listened to here rather than through Blazor, so that only Escape is sent to the server.
    // A Blazor handler would receive every key pressed inside the popup and re-render the popup with its
    // content, handing components like SpinEdit their parameters again while they still handle the same
    // key press.
    readonly #modalDialogKeyDown = (event: KeyboardEvent) => {
        if (event.key === 'Escape')
            void this.#dotNetObject.invokeMethodAsync('CloseAsync');
    };

    constructor(dotNetObject: DotNet.DotNetObject, modalDialogElement: HTMLElement) {
        this.#dotNetObject = dotNetObject;
        this.#modalDialogElement = modalDialogElement;

        this.#modalDialogElement.addEventListener('keydown', this.#modalDialogKeyDown);
    }

    public dispose() {
        this.#modalDialogElement.removeEventListener('keydown', this.#modalDialogKeyDown);
    }
}
