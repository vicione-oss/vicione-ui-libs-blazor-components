// `@stylistic/operator-linebreak` only checks operators between two types, so it never sees an optional
// leading `|` or `&`. Its fix even leaves one behind when it converts an operator-first union, mixing both
// styles in one type.
const noLeadingTypeOperator = {
    meta: {
        type: 'layout',
        fixable: 'whitespace',
        schema: [],
        messages: {
            unexpected: 'Unexpected leading \'{{operator}}\'; place type operators at the end of the line.'
        }
    },
    create(context) {
        const { sourceCode } = context;

        const check = node => {
            const [firstType] = node.types;
            const token = sourceCode.getTokenBefore(firstType);
            if (!token || token.range[0] < node.range[0] || (token.value !== '|' && token.value !== '&')) {
                return;
            }

            context.report({
                node: token,
                messageId: 'unexpected',
                data: { operator: token.value },
                fix(fixer) {
                    // Removing the operator would also remove a comment written after it
                    if (sourceCode.commentsExistBetween(token, firstType)) {
                        return null;
                    }

                    return fixer.removeRange([token.range[0], firstType.range[0]]);
                }
            });
        };

        return {
            TSUnionType: check,
            TSIntersectionType: check
        };
    }
};

export default noLeadingTypeOperator;
