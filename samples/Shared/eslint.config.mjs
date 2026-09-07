import rootConfig from '../../eslint.config.mjs';

const config = [
    ...rootConfig,

    {
        ignores: [
            'dist/**/*.js',
            'wwwroot/css/site.css'
        ]
    }
];

export default config;
