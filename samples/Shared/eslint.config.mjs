import rootConfig from '../../eslint.config.mjs';

const config = [
    ...rootConfig,

    {
        ignores: [
            'dist/**/*.js',
            'wwwroot/css/site.css'
        ]
    },

    {
        files: ['package.json'],
        rules: {
            // A workspace runs on the Node.js version declared by `engines.node` of the root package
            'package-json/require-engines': 'off'
        }
    }
];

export default config;
