import {Component, OnInit} from '@angular/core';
import {DataService} from "./Dataservice/dataservice.service";
import {map} from "rxjs/operators";
import * as FileSaver from 'file-saver';
import {MainComponent} from "./main/main.component";
import { HttpEvent, HttpEventType } from '@angular/common/http';
import { Location } from '@angular/common'
import  './Dataservice/Types.ts';

@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],

})
export class AppComponent implements OnInit{


  rootPath = "/"

  private _message = ""
  get message(): string {
    return this._message;
  }

  set message(value: string) {
    this._message = this._errorMessage == ""
                    ? value
                    : ""
  }

  pathNodes : PathNode[] = []
  newFolderPNode?:PathNode = undefined
  customerCode: string = "";
  messageTimeout = 1500 // milliseconds

  _errorMessage = ""
  get errorMessage() { return this._errorMessage }
  set errorMessage(val) {
    this._errorMessage = val
    if (val != "") {
      this._message = "" // clear normal message when error message is active
      setTimeout(() => this._errorMessage = "", this.messageTimeout) // clear error afterwards
    }
  }

  // sets the normal message but clears it after sometime
  autoResetMsg (message:string) {
    this.message = message
    setTimeout(() => this.message = "", this.messageTimeout)
  }

  _currentFolder : string = this.rootPath

  inputFileNames: any;
  inputFolderNames: any;

  Separator = "/"

  get showBackArrow() {
    return this.currentFolder != this.Separator
  }
  get currentFolder() {
      return this._currentFolder
  }
  set currentFolder(val) {
    if (val == '' )  val = this.Separator

    if (! val.startsWith(this.Separator))  val = this.Separator + val
    if (! val.endsWith(this.Separator)) val += this.Separator

    this._currentFolder = val

  }


  getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
  }


  constructor(private _data: DataService, private location:Location) {

  }

  removeTrailingSlash(str:string) {
    return str.endsWith("/")
            ? str.slice(0, str.length - 1)
            : str
  }

  removeStartingSlash(str:string){
    return str.startsWith("/")
            ? str.slice(1,str.length)
            : str
  }

  cmpFolders (a:PathNode, b:PathNode)
  {
    if (a.isFolder && !b.isFolder) return -1
    if (!a.isFolder && b.isFolder) return 1
    if (a.isFolder && b.isFolder ||
       !a.isFolder && !b.isFolder) {
      return (a.createdOn > b.createdOn) ? 1 : -1
    }
    return 0
  }

  refresh() {
    this.newFolderPNode = undefined
    this.inputFolderNames = null
    this.inputFileNames = null
    this.message = "Fetching ..."
    // this.errorMessage = ""

    console.log (`Refreshing ...`)
    if (this.customerCode == "") {
      this._data.GetCustomerCode()
        .subscribe(
          {
            next: customerCode => {
              // console.log(`Customer code assigned ${customerCode}`)
              this.customerCode = customerCode
            },
            error: err => {
              console.log(`Error : ${err.message}`)
              this.customerCode = ""
            },
            complete: () => {
              // console.log("completed")
            }
          })
    }
    this.currentFolder = this.getCurrentPathName()
    if (this.currentFolder === "") { this.currentFolder = "/"}
    let currentFolderLength = this.currentFolder.length
    this._data.GetFileList(this.currentFolder)
      .pipe(
        map(nodes => nodes.map ( node => {
          node.name = node.name.slice(currentFolderLength) // remove the path from the filename
          node.name = this.removeStartingSlash(node.name)
          return node
        } )
        ),
        map(nodes => nodes.sort( (a:PathNode,b:PathNode) => this.cmpFolders(a,b)) )
      )
      .subscribe({
        next: nodes => this.pathNodes = nodes,
        error: err => {
          this.errorMessage = ` ${this.currentFolder}   not found.`
          this.doChangeFolder("")
        },
        complete: () => this.message = ""
      })
  }

  combinePaths (path1:string, path2:string) {
    if (path1 == "") { return path2}
    if (path2 == "") { return path1}
    let p1 = path1.endsWith("/")
    let p2 = path2.startsWith("/")

    if (p1 && p2 ) // both has slashes
      return `${this.removeTrailingSlash(path1)}${path2}`
    else if (p1 && !p2  || !p1 && p2 ) // only one of them has a slash
      return path1 + path2

    return  `${path1}/${path2}` // default

  }

  downloadProgress( fullFileName:string, ev:HttpEvent<Blob>) {
    if (ev.type === HttpEventType.DownloadProgress) {
      if (ev.loaded && ev.total) {
        const percentDone = Math.round(100 * ev.loaded / ev.total)
        if (ev.loaded >= ev.total) { // done ?
          this.message = `Downloaded ${fullFileName} - Saving`
        }
        else {
          this.message = `Downloading ${fullFileName} ${percentDone}%`
        }
      }
    }
  }

  removeQuotes (fn:string)
  {
    if (fn.startsWith('"') && fn.endsWith('"'))
      return fn.slice(1, -1);
    else return fn
  }


  // get the suggested filename from content disposition header
  // it is preferable to get it from UTF-8 encoded field name filename*
  // otherwise take it from filename field
  // the value may or may not be quoted. if quoted remove the quote
  getFilenameFromContentDisposition(contentDisposition: string): string  {
    const match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/);
    if (match && match.length > 1) {
        return this.removeQuotes ( decodeURIComponent(match[1]) );

    }
    const filenameMatch = contentDisposition.match(/filename="([^;]+)/);
    if (filenameMatch && filenameMatch.length > 1) {
      return this.removeQuotes (filenameMatch[1]);

    }
    return ""; // string empty if content disposition is empty
  }


  fileDownLoad($event: PathNode) {
    let fileName = $event.name
    let fileDate = $event.createdOn
    let fullFileName = this.combinePaths(this.currentFolder, fileName)
    this.message = `Preparing to download ${fullFileName} ...`
    this._data.DownloadFile(fullFileName, fileDate)
      .subscribe(
        {
          next: dwnLdProgressEvent => {
            this.downloadProgress(fullFileName, dwnLdProgressEvent)
            if(dwnLdProgressEvent.type === HttpEventType.Response) {
              const contentDisposition = dwnLdProgressEvent.headers.get('content-disposition');
              if (contentDisposition) { // if content disposition is present use the file name present in this 
                const filename = this.getFilenameFromContentDisposition(contentDisposition);
                if (dwnLdProgressEvent.body) FileSaver.saveAs(dwnLdProgressEvent.body, filename)
              }
              else { // no content disposition use the file name that was originally displaed on the browser
                if (dwnLdProgressEvent.body) FileSaver.saveAs(dwnLdProgressEvent.body, fileName)
              }

            }
          },
          error: err => this.errorMessage = `Error: Download ${fullFileName} - ${err.message}`,
          complete:() => this.autoResetMsg( `Downloaded ${fullFileName}`)
        }
      )
  }

  getCurrentPathName()
  {
    // console.log( `window.location.href ${window.location.pathname}`)
    return decodeURIComponent (window.location.pathname)
  }

  doChangeFolder(newFolder:string) {
      let oldFolder = this.currentFolder
      this.currentFolder = newFolder.trim()
      if( oldFolder != this.currentFolder ) {
        // console.log(`Changing folder from --${oldFolder}-- to --${newFolder.trim()}--  `)

        this.location.replaceState(this.currentFolder)

        this.refresh()
      }

  }

  changeFolder($event: string) {
    this.doChangeFolder(this.currentFolder + $event)
    // this.currentFolder +=  $event
    // console.log (`Changing folder to ${this.currentFolder}`)
    // this.refresh()
  }

  changeToParentFolder() {
     if (this.currentFolder == "/") {
       this.refresh()
       return;
     }
    let str = this.removeTrailingSlash(this.currentFolder)
    let toks = str.split("/")
    let pth = toks.slice(0, toks.length-1)
    this.doChangeFolder(pth.join("/"))
    // this.currentFolder = pth.join("/")
    // console.log (`Changing folder to ${this.currentFolder}`)
    //
    // this.refresh()

  }



  removeLeadingAndTrailingSlash(str:string) {
    return this.removeTrailingSlash(
           this.removeStartingSlash(str)
    )
  }


invalidCharsForFileName = new Set(['/', '\\', '?', '%', '*', '|', '"', '<', '>']);

validNewName(fileName: string): boolean {
  return !Array
    .from(fileName)
    .some(ch => this.invalidCharsForFileName.has(ch))
    ;
  }

  // if (reservedNames.has(fileName.toLowerCase())) {
  //   return false;
  // }

//   return true;
// }

  // validNewName(fileName: string): boolean {
  //   const invalidCharsRegex = /[/\\?%*:|"<>]/g //   /[<>:"\/\\|?*\x00-\x1F]/g; // Regex to match invalid characters /[/\\?%*:|"<>]/g
  //   const reservedNames = ['con', 'prn', 'aux', 'nul', 'com1', 'com2', 'com3', 'com4', 'com5', 'com6', 'com7', 'com8', 'com9', 'lpt1', 'lpt2', 'lpt3', 'lpt4', 'lpt5', 'lpt6', 'lpt7', 'lpt8', 'lpt9']; // Reserved file names

  //   if (fileName.match(invalidCharsRegex)) {
  //     return false; // Contains invalid characters
  //   }

  //   if (reservedNames.includes(fileName.toLowerCase())) {
  //     return false; // Matches a reserved name
  //   }

  //   return true;
  // }

  //
  // validNewName (fname:string) {
  //   if (fname == "") return false
  //   if (fname == "." || fname == "..") return false
  //   const Allowed = `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-()+0123456789.~ `
  //   return Array.from(fname).every(s => Allowed.indexOf(s) != -1)
  // }

  newFolderButtonClicked() {
    this.newFolderPNode =
      {
        name : "",
        isFolder : true,
        size : 0,
        createdOn : new Date()
      }
  }

  doCancelNewFolder() {
    this.newFolderPNode = undefined
    this.refresh();
  }

  doNewFolder($event: string) {
    this.newFolderPNode = undefined
    if ($event == "") {
      this.refresh();
      return;
    }
    if (! this.validNewName($event)) {
      this.errorMessage = "Invalid characters in folder name";
      this.refresh();
      return
    }
    const newFolderPath = this.combinePaths(this.currentFolder, $event)
    this.message = `Creating new folder ${newFolderPath} `
    this.errorMessage = ""
    // console.log (`creating NewFolder  ${newFolderPath}`)
    this._data.CreateNewfolder(newFolderPath)
      .subscribe({
          next: _str => {},
          error: err => this.errorMessage = `Error : New folder ${newFolderPath} - ${err.message}`,
          complete: () => this.refresh()
        }
      )
  }

  uploadProgress( files:FileList, ev:HttpEvent<any>) {

    if (ev.type === HttpEventType.UploadProgress) {
      let fileName = files.length == 1 ? files[0].name : `${files.length} Files.`

      if (ev.loaded && ev.total) {
        const percentDone = Math.round(100 * ev.loaded / ev.total)
        if (percentDone > 99.0) { // done ?
          this.message = `Uploaded ${fileName} - Saving`
        }
        else {
          this.message = `Uploading ${fileName} ${percentDone}%`
        }
      }
    }
  }


  doFileUpload(files: FileList | null) {
    // console.log("doFileUpload ", files)
    if (files && files.length > 0) {
      for (let i = 0; i < files.length ; i++) {
        if (! this.validNewName(files[i].name)) {
          this.errorMessage = `Invalid file name ${files[i].name}`
          return
        }
      }

      this.message = files.length > 1
                      ? `Uploading ${files.length} files`
                      : `Uploading ${files[0].name}`


      this._data.UploadFiles(this.currentFolder, files)
        .subscribe(
          {
            next: n => { this.uploadProgress(files, n)},
            error: err => this.errorMessage = 'Error: File upload : ' + err.message,
            complete: () => {
              this.refresh()

            }
          });
    }
  }

  doLogout() {
    MainComponent.LogOut()
  }

  ngOnInit(): void {
    this.refresh()
  }

  deleteFileFolder($event: PathNode) {
    if ( !$event ) return
    let fullName = this.combinePaths(this.currentFolder, $event.name)
    // console.log(`Delete  ${fullName}`)

    if ($event.isFolder) {
      this._data.DeleteFolder(fullName)
        .subscribe(
          {
            next: s => {console.log(`delete  folder: `,s); },
            error: err => {console.log(`delete folder Error: `, err.message)},
            complete: () => {this.refresh()}
          }
        )
    } else {
      this._data.DeleteFile(fullName)
        .subscribe(
          {
            next: s => {console.log(`delete file : `,s); },
            error: err => {console.log(`delete file Error: `, err.message)},
            complete: () => {this.refresh()}
          }
        )
      }
    }

}



