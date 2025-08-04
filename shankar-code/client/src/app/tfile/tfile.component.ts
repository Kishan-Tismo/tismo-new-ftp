import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {DataService} from "../Dataservice/dataservice.service";

@Component({
  selector: 'app-tfile',
  templateUrl: './tfile.component.html',
  styleUrls: ['./tfile.component.scss']
})
export class TFileComponent implements OnInit{

  @Input()
  file: PathNode = {name: "/", isFolder: true, size: 0, createdOn: new Date(1970, 1, 1)}

  @Input()
  currentFolder:string = ""

  @Input()
  newFolder:boolean = false

  @Output() fileDownLoad = new EventEmitter<PathNode>()

  @Output() changeFolder = new EventEmitter<string>()


  @Output() createNewFolder = new EventEmitter<string>()
  @Output() cancelNewFolder = new EventEmitter()

  @Output() deleteFileFolder = new EventEmitter<PathNode>()


  getHref(filename:string) {
    let fileWithPath = this.currentFolder + filename
    return this.data.getHref(fileWithPath)
  }

  fileIconMap =
    [
      [".zip","compress"],
      [".pdf", "picture_as_pdf"],
      [".doc", "notes"],
      [".txt","notes"],
      [".jpg","image"],
      [".png","image"],
      [".html","html"],
      [".js","javascript"],
      [".css","css"],
      [".c", "code"],
        [".cs", "code"],
        [".cpp", "code"],
        [".h", "code"],
        [".c", "code"],
        [".fs", "code"],
      [".ts", "code"],

    ]
  newFolderName:string = ""
  fileIcon: string = ""

  getFileIcon(filename:string) {
    let tp = this.fileIconMap.find(tup => filename.endsWith(tup[0]) )
    return tp ? tp[1] : "description"
  }

  constructor(private data:DataService) {

  }

  ngOnInit() {
    if(! this.file.isFolder)
      this.fileIcon = this.getFileIcon(this.file.name)

    if (this.newFolder) {
      let elem = document.getElementById("newFolderNameInput")
      if (elem) {
        elem.focus()
      }
    }
  }




  fileSizeStr(number:number) {
    const oneKb = 1024;
    const oneMb = oneKb * oneKb;
    const oneGb = oneMb * oneKb;
    const oneTb = oneGb * oneKb;

    if(number < oneKb) {
      return number + ' bytes';
    }
    if(number >= oneKb && number < oneMb) {
      return (number/oneKb).toFixed(1) + ' KB';
    }
    if(number >= oneMb && number < oneGb) {
      return (number/oneMb).toFixed(2) + ' MB';
    }
    if(number >= oneGb && number < oneTb ) {
      return (number/oneGb).toFixed(2) + ' GB';
    }
    if(number >= oneTb) {
      return (number/oneTb).toFixed(2) + ' TB';
    }
    return ''
  }

  deleteFileClicked() {
    let msg = this.file.isFolder ? `Delete Folder '${this.file.name}' and all its content ? `
                               : `Delete File '${this.file.name}' ?`
    const result = confirm(msg); // delete file / folder ?
    if (result) {
      this.deleteFileFolder.emit(this.file)
    }

  }

  deleteHoverActive = false; // mouse is on the delete file icon
  deleteHoverLeave() {
    this.deleteHoverActive = false;
  }

  deleteHoverEnter() {
    this.deleteHoverActive = true;
  }
}
