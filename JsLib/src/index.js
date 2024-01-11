import * as React from 'react';
import ReactDOM from 'react-dom';
import { IDisposable, MessageProcessor, PowerFxFormulaEditor } from '@microsoft/power-fx-formulabar/lib';

let _messageProcessor;
let _editor;
let _listener;

const onDataReceived = (data) => {
    _listener(data);
};

export function showEditor(initialValue) {

    console.log('showing editor');    

    const messageProcessor = {

        addListener: (listener) => {

            _listener = listener;

            return {

                dispose: () => null
            };

        },

        sendAsync: async (data) => {
            //console.log(data)
            notifyBlazorSendAsync(data);
        }

    };

    ReactDOM.render(
  
        <PowerFxFormulaEditor            
            getDocumentUriAsync={getDocumentUriAsync}
            defaultValue={initialValue}
            messageProcessor={messageProcessor}
            maxLineCount={10}
            minLineCount={4}
            onChange={(newValue) => {
                notifyBlazorFormulaHasChanged(newValue);
            }}
            onEditorDidMount={(editor, _) => {
                _editor = editor;
            }}
            lspConfig={{
                enableSignatureHelpRequest: true
            }}
        />

    , document.getElementById('formula'));

}

async function notifyBlazorSendAsync(data) {

    try {

        var response = await DotNet.invokeMethodAsync('PowerFxBlazorDemo', 'LanguageServerService', data);

        const responseArray = JSON.parse(response);

        responseArray.forEach((item) => {
            //console.log('[LSP Client] Receive: ' + item);
            onDataReceived(item);
        })

    } catch (error) {

        console.error(error);

    }

}

async function notifyBlazorFormulaHasChanged(data) {

    try {

        await DotNet.invokeMethodAsync('PowerFxBlazorDemo', 'FormulaHasChanged', data)

    } catch (error) {

        console.error(error);

    }

}

async function getDocumentUriAsync() {

    try {

        const context = await DotNet.invokeMethodAsync('PowerFxBlazorDemo', 'GetCurrentContext');

        return 'powerfx://demo?context=' + context;

    } catch (error) {

        console.log('context error:' + error);

        return ""

    }

}




