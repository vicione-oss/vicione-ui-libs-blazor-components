import eslintConfigXo from 'eslint-config-xo';

const indent = 4;

const config = [
    {
        ignores: [
            '**/bin/**',
            '**/obj/**',
            '**/*.cs.js',
            '**/*.razor.js',
            '**/*.razor.css',
            '**/*.html',
            '**/*.json',
            '**/*.md'
        ]
    },
    ...eslintConfigXo({ space: indent }),
    {
        languageOptions: {
            parserOptions: {
                // Pin type resolution to the solution root, so linting a project
                // does not depend on the directory ESLint is started from
                tsconfigRootDir: import.meta.dirname,

                projectService: {
                    // No `tsconfig.json` covers the configuration files themselves
                    allowDefaultProject: [
                        'eslint.config.mjs',
                        'src/*/eslint.config.mjs',
                        'samples/*/eslint.config.mjs'
                    ]
                }
            }
        }
    },
    {
        files: ['**/*.ts'],
        rules: {
            'no-unused-vars': 'error',
            curly: ['error', 'multi-or-nest', 'consistent'],
            '@stylistic/padded-blocks': 'off',
            '@stylistic/indent': ['error', indent],
            '@stylistic/indent-binary-ops': ['error', indent],
            '@stylistic/max-len': [
                'error',
                {
                    code: 140,
                    ignoreComments: true,
                    ignoreUrls: true,
                    ignorePattern: '^import '
                }
            ],
            '@stylistic/comma-dangle': ['error', 'never'],
            '@stylistic/function-paren-newline': ['error', 'consistent'],
            '@stylistic/curly-newline': ['error', { minElements: 1 }],
            '@stylistic/object-curly-spacing': ['error', 'always'],
            '@stylistic/operator-linebreak': ['error', 'after'],
            '@typescript-eslint/no-empty-object-type': ['error', { allowInterfaces: 'with-single-extends' }],
            'import-x/no-absolute-path': 'off',
            'import-x/no-unassigned-import': ['error', { allow: ['**/*Mixins.ts', '**/*Mixins.js'] }],
            'unicorn/prefer-number-coercion': 'off',
            'unicorn/filename-case': 'off',
            'unicorn/no-non-function-verb-prefix': 'off',
            'unicorn/consistent-boolean-name': [
                'error',
                {
                    prefixes: {
                        include: true,
                        process: true,
                        snapped: true
                    }
                }
            ]
        }
    },

    // Mixin classes use the `this` parameter pattern, so members referenced via
    // `this` are declared on the target type, not on the mixin class itself.
    {
        files: ['**/*Mixins.ts'],
        rules: {
            'unicorn/no-undeclared-class-members': 'off'
        }
    },

    // Linting rule adjustments for the configuration files themselves
    {
        files: ['**/eslint.config.mjs'],
        rules: {
            '@typescript-eslint/no-unsafe-assignment': 'off',
            '@typescript-eslint/no-unsafe-call': 'off',
            '@typescript-eslint/no-unsafe-member-access': 'off',
            '@typescript-eslint/naming-convention': 'off',
            'unicorn/filename-case': 'off',
            '@stylistic/comma-dangle': ['error', 'never'],
            '@stylistic/object-curly-spacing': ['error', 'always']
        }
    }
];

export default config;
