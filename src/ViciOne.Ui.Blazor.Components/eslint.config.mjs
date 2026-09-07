import rootConfig from '../../eslint.config.mjs';

const config = [
    ...rootConfig,

    {
        ignores: [
            'Scripts/*.js',
            'dist/**/*.js',
            'obj/types/**/*.d.ts'
        ]
    }
];

export default config;
