import { type PointerCaptureBehavior } from './PointerCaptureBehavior.ts';
import { type PointerCaptureBehaviorContext } from './PointerCaptureBehaviorContext.ts';
import { PointerCaptureBehaviorPipeline } from './PointerCaptureBehaviorPipeline.ts';

export class AggregatePointerCaptureBehavior implements PointerCaptureBehavior {

    constructor(readonly innerBehaviors: Iterable<PointerCaptureBehavior>) {}

    public apply(context: PointerCaptureBehaviorContext, next: (context: PointerCaptureBehaviorContext) => void) {
        const innerPipeline = new PointerCaptureBehaviorPipeline(this.innerBehaviors);
        innerPipeline.start(context);

        next(context);
    }
}
