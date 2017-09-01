const path = require('path');
const webpack = require('webpack');
const CopyWebpackPlugin = require('copy-webpack-plugin');

const Assets = [{
    origin: "bootstrap/dist/",
    target: "bootstrap"
}, {
    origin: "jquery/dist/",
    target: "jquery"
}, {
    origin: "jquery-validation/dist/",
    target: "jquery-validation"
}, {
    origin: "jquery-validation-unobtrusive/jquery.validate.unobtrusive.js",
    target: "jquery-validation-unobtrusive"
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
                    to: path.resolve(__dirname, `./dependencies/${asset.target}`)
                };
            })
        )
    ]
};