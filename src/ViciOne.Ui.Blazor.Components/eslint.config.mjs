import xoTypeScript from 'eslint-config-xo-typescript';

// https://github.com/xojs/xo/issues/798
const xoTypeScriptPatched = xoTypeScript.filter(config => config.language?.startsWith('json/') !== true);

export default [
    {
        ignores: [
            'Scripts/*.js',
            '**/*.cs.js',
            '**/*.razor.js',
            'wwwroot/context-menu/**/*.js',
            'wwwroot/js/array-iterator.js',
            'wwwroot/js/point.js',
            'wwwroot/js/pointer-event-mixins.js',
            'wwwroot/moveable/*.js',
            'wwwroot/pointer-capture/*.js',
            'wwwroot/text-box/**/*.js',
            'wwwroot/tooltip/**/*.js',
            'wwwroot/resizing/*.js',
            'wwwroot/breadcrumb/*.js'
        ]
    },
    ...xoTypeScriptPatched,
    {
        languageOptions: {
            parserOptions: {
                projectService: {
                    allowDefaultProject: ['eslint.config.mjs']
                }
            }
        }
    },
    {
        rules: {
            'no-unused-vars': 'error',
            curly: ['error', 'multi-or-nest', 'consistent'],
            '@stylistic/padded-blocks': 'off',
            '@stylistic/indent': ['error', 4],
            '@stylistic/indent-binary-ops': ['error', 4],
            '@stylistic/comma-dangle': ['error', 'never'],
            '@stylistic/function-paren-newline': ['error', 'consistent'],
            '@stylistic/curly-newline': ['error', { minElements: 1 }],
            '@stylistic/object-curly-spacing': ['error', 'always'],
            '@typescript-eslint/no-empty-object-type': ['error', { allowInterfaces: 'with-single-extends' }]
        }
    },

    // Linting rule adjustments for this file
    {
        files: ['eslint.config.mjs'],
        rules: {
            '@typescript-eslint/no-unsafe-assignment': 'off',
            '@typescript-eslint/no-unsafe-call': 'off',
            '@typescript-eslint/no-unsafe-member-access': 'off',
            '@typescript-eslint/naming-convention': 'off'
        }
    }
];
