
const path = require("path");

module.exports = {
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [
                    'style-loader',
                    'css-loader'
                ]
            },
            {
                test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                use: {
                    loader: "babel-loader"
                }
            }
        ]
    },
    optimization: {
        minimize: false,
    },
    output: {
        path: path.resolve(__dirname, '../wwwroot/js'),
        filename: "my_lib.js",
        library: "MyLib"
    }
};
