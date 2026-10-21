import rootConfig from '../../eslint.config.mjs';

const config = [
    ...rootConfig,

    {
        ignores: [
            'Scripts/*.js',
            'dist/**/*.js',
            'obj/types/**/*.d.ts'
        ]
    },

    {
        files: ['package.json'],
        rules: {
            // A workspace runs on the Node.js version declared by `engines.node` of the root package
            'package-json/require-engines': 'off',

            // `@types/blazor__javascript-interop` declares the global `DotNet` object provided by
            // Blazor at runtime, so it has no runtime package to go with, hence this rule is off
            'package-json/no-orphan-types': 'off'
        }
    }
];

export default config;
