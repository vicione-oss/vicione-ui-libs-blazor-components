import { type CaptureTargetRect } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target-rect.js';

export class CaptureTarget {
    readonly originalRect: Readonly<CaptureTargetRect>;
    lastRect: Readonly<CaptureTargetRect>;

    constructor(readonly element: HTMLElement, readonly rect: CaptureTargetRect) {
        this.originalRect = rect.copy();
        this.lastRect = rect.copy();
    }

    public saveRect() {
        this.lastRect = this.rect.copy();
    }
}
