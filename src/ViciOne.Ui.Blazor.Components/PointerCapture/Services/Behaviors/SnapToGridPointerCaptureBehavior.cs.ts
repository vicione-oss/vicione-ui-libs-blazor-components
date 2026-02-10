import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';

class SnapToGridPointerCaptureBehavior implements PointerCaptureBehavior {
    private gridSize = 10;

    constructor(gridSize?: number) {
        if (gridSize)
            this.gridSize = gridSize;
    }

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        let positionChanged = false;

        let remainder = context.position.x % this.gridSize;
        if (remainder !== 0) {
            context.position.x -= remainder;

            positionChanged = true;
        }

        remainder = context.position.y % this.gridSize;
        if (remainder !== 0) {
            context.position.y -= remainder;

            positionChanged = true;
        }

        if (!positionChanged)
            next(context);
    }

    public setGridSize(value: number) {
        this.gridSize = value;
    }
}

export async function createInstance(gridSize?: number) {
    return new SnapToGridPointerCaptureBehavior(gridSize);
}
