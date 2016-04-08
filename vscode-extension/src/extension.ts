'use strict';
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';

var ipc;

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {

    // Use the console to output diagnostic information (console.log) and errors (console.error)
    // This line of code will only be executed once when your extension is activated
    console.log('Congratulations, your extension "hsp-rtm" is now active!');

    var watcher = new DocumentWatcher(context);

    // The command has been defined in the package.json file
    // Now provide the implementation of the command with  registerCommand
    // The commandId parameter must match the command field in package.json
    let disposable = vscode.commands.registerCommand('hsprtm.toggleRtm', () => {
        // The code you place here will be executed every time your command is executed

        // Display a message box to the user
        watcher.toggle();
        vscode.window.showInformationMessage('HSP Real-Time Debugging!');
        
        var spawn = require('child_process').spawn;

        ipc = spawn(process.env[(process.platform == 'win32') ? 'USERPROFILE' : 'HOME'] + "\\.vscode\\extensions\\hsp-rtm\\hsp.watcher.exe");
        ipc.stdin.setEncoding("utf8");
    });

    context.subscriptions.push(disposable);
}

// this method is called when your extension is deactivated
export function deactivate() {
}

export default class DocumentWatcher implements vscode.Disposable {
    private _context: vscode.ExtensionContext;
    private _isEnabled: boolean;
    
    constructor(context: vscode.ExtensionContext) {
        this._context = context;
        this._isEnabled = false;
        vscode.workspace.onDidChangeTextDocument(this.onTextDocumentChanged, this, context.subscriptions);
        
    }
    
    onTextDocumentChanged(arg: vscode.TextDocumentChangeEvent){
        const self = this;
        if (!self._isEnabled) return;
        
        var code = arg.document.getText().replace(/\r?\n/g, 'Producer-San, Is a new line! new line!!');

        ipc.stdin.write(code + "\n");
        var stdin = process.stdin;
        stdin.on('data', function (data) {
            ipc.stdin.write(data);
        });
    }
    toggle() {
        if (this._isEnabled) this.stop();
        else this.start();
    }
    start() {
        this._isEnabled = true;
    }
    stop() {
        this._isEnabled = false;
    }
    dispose() {
    }   
}