const path = require('path');
const webpack = require('webpack');
const CopyWebpackPlugin = require('copy-webpack-plugin');

const Assets = [{
    origin: 'bootstrap/dist/',
    target: 'bootstrap'
}, {
    origin: 'jquery/dist/',
    target: 'jquery'
}, {
    origin: 'jquery-validation/dist/',
    target: 'jquery-validation'
}, {
    origin: 'jquery-validation-unobtrusive/jquery.validate.unobtrusive.js',
    target: 'jquery-validation-unobtrusive'
}, {
    origin: 'jquery-ajax-unobtrusive/jquery.unobtrusive-ajax.js',
    target: 'jquery-ajax-unobtrusive'
}, {
    origin: 'jquery-ajax-unobtrusive/jquery.unobtrusive-ajax.min.js',
    target: 'jquery-ajax-unobtrusive'
}, {
    origin: 'countdown/countdown.js',
    target: 'countdown'
}, {
    origin: 'codemirror/',
    target: 'codemirror'
},{
    origin: 'diff_match_patch/',
    target: 'diff_match_patch'
}];

module.exports = {
    entry: {
        app: './Scripts/global.js'
    },
    output: {
        path: __dirname + '/Dependencies/',
        filename: '[name].bundle.js'
    },
    plugins: [
        new CopyWebpackPlugin(
            Assets.map(asset => {
                return {
                    from: path.resolve(__dirname, `./node_modules/${asset.origin}`),
                    to: path.resolve(__dirname, `./Dependencies/${asset.target}`)
                };
            })
        )
    ]
};