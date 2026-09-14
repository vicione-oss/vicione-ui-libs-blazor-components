import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';

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
