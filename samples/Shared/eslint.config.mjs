import generalConfig from '../../eslint.config.mjs';

const config = [
    ...generalConfig,

    {
        ignores: [
            'dist/**/*.js',
            'wwwroot/css/site.css'
        ]
    }
];

export default config;
