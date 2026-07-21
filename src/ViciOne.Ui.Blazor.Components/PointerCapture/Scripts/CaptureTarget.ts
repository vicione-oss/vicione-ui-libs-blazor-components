import { type CaptureTargetRect } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/capture-target-rect.js';

/**
 * Describes the pointer capture target in its current state.
 *
 * Behaviors should use {@link Readonly} parameters like {@link CaptureTarget.originalRect} as the source to
 * calculate new state and update writeable parameters like {@link CaptureTarget.rect} from it when needed.
 */
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
