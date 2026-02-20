import { type Point } from '/_content/ViciOne.Ui.Blazor.Components/js/point.js';

export class PointerCaptureBehaviorContext {
    constructor(readonly captureTarget: HTMLElement, readonly position: Point) {}
}
