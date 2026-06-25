import { type PointerCaptureBehavior } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior.js';
import { type PointerCaptureBehaviorContext } from '/_content/ViciOne.Ui.Blazor.Components/pointer-capture/pointer-capture-behavior-context.js';

export class PointerCaptureBehaviorPipeline {

    constructor(private readonly behaviors: Iterable<PointerCaptureBehavior>) {}

    public start(context: PointerCaptureBehaviorContext): void {
        const iterator = this.behaviors[Symbol.iterator]();

        const next = (behaviorContext: PointerCaptureBehaviorContext) => {
            const result = iterator.next();
            if (result.done)
                return;

            result.value.apply(behaviorContext, next);
        };

        next(context);
    }
}
