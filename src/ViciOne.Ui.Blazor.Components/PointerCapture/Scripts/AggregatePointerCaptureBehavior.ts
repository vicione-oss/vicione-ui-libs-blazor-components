import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';
import { PointerCaptureBehaviorPipeline } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-pipeline.js';

export class AggregatePointerCaptureBehavior implements PointerCaptureBehavior {

    constructor(readonly innerBehaviors: Iterable<PointerCaptureBehavior>) {}

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const innerPipeline = new PointerCaptureBehaviorPipeline(this.innerBehaviors);
        innerPipeline.start(context);

        next(context);
    }
}
