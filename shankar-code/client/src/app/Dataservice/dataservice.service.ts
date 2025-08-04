import {Inject, Injectable} from '@angular/core';
import {HttpClient, HttpEvent, HttpEventType, HttpHeaders, HttpParams} from "@angular/common/http";
import {Observable} from "rxjs";
import {map} from "rxjs/operators";
// import { encode } from 'querystring';

@Injectable({
  providedIn: 'root'
})
export class DataService  {
  private baseServerAPIUrl: string;
  isformdata = false;
  static authString = '';

  // get formDataHttpOptions() {
  //   return {
  //     headers: new HttpHeaders({
  //       // 'Content-Type': 'multipart/form-data',
  //       Authorization: 'Bearer ' + DataService.authString
  //     })
  //   };
  // }

  get httpOptions() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + DataService.authString
      })
    };
  }

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    // this.baseRawUrl = baseUrl;
    this.baseServerAPIUrl = baseUrl.replace('4200', '5000') ; // 4200 to 5000 is for development machine
  }

  setHttpAuthorizationHeader(authString: string) {
    DataService.authString = authString;
  }

  Login(username:string, password:string) : Observable<string> {
    const params= new HttpParams({
      fromObject: {
        user:username,
        pwd:password
      }
    })
    return this.http.get<string>(this.baseServerAPIUrl + "login",
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        }),
        params: params
      }
    )
  }

  DeleteFile(fileName:string) : Observable<string> {
    let url = this.baseServerAPIUrl + "deleteFile/" + encodeURIComponent(fileName)
    return this.http.delete<string>(url, {
      headers:this.httpOptions.headers
    });
  }

  DeleteFolder(folderName:string): Observable<string> {
    let url = this.baseServerAPIUrl + "deleteFolder/" + encodeURIComponent(folderName)
    return this.http.delete<string>(url, {
      headers:this.httpOptions.headers
    });
  }

  CreateNewfolder(folder:string) : Observable<string> {
    const params = new HttpParams( {
      fromObject: {
        folderName:encodeURI(folder)
      }
    })
    const formData: FormData = new FormData();
    return this.http.post<string>(this.baseServerAPIUrl + "createFolder",
      formData,
      {
        headers:this.httpOptions.headers,
        params:params
      }

      )
  }


  getHref(filepath:string) {
    return `${this.baseServerAPIUrl}download?filename=${encodeURI(filepath)}`
  }

  UploadFiles(folder:string, filesToUpload: FileList) : Observable<HttpEvent<any>> {
    const params = new HttpParams({
      fromObject: {
        folderName: encodeURI(folder)
      }
    }); // query parameters
    const endpoint = this.baseServerAPIUrl + 'upload';
    const formData: FormData = new FormData();
    for (let i = 0; i < filesToUpload.length; i++) {
      let fileName =
        filesToUpload[i].webkitRelativePath.length != 0 ? // prefer full path
                    filesToUpload[i].webkitRelativePath
                    : filesToUpload[i].name // else just the filename
      formData.append('file', filesToUpload[i], fileName);
      console.log(filesToUpload[i])
    }

     //  .pipe(catchError(this.handleError));
    return this.http.post(endpoint, formData,
      {
        headers: new HttpHeaders({
          // 'Content-Type': 'multipart/form-data',
          Authorization: 'Bearer ' + DataService.authString
        }),
        params: params,
        reportProgress: true,
        observe:'events'


      })
  }

  DownloadFile(fileToDownload: string, fileDate: Date): Observable<HttpEvent<Blob>> {
    let fileDateStr = fileDate.toISOString()
    const params = new HttpParams({ fromObject: { filename: encodeURI(fileToDownload), fileDate: encodeURIComponent(fileDateStr) }   })
    return this.http.get(
      this.baseServerAPIUrl + "download",
      {
        headers: this.httpOptions.headers,
        params: params,
        responseType: 'blob',
        reportProgress: true,
        observe: 'events'
      }
      )
  }

  GetCustomerCode() : Observable<string> {
    return this.http.get<string>(
      this.baseServerAPIUrl + "customerCode",
      { headers: this.httpOptions.headers }
      )
  }

  GetFileList(folder:string = "/"): Observable<PathNode[]> {
    const params = new HttpParams({
      fromObject: {
        folderName: encodeURI(folder)
      }
    }); // query parameters
    return this.http.get<PathNode[]>(this.baseServerAPIUrl + "fileList",
      {
        headers: this.httpOptions.headers,
        params: params
      }
      )
      .pipe(
        map(pnodes =>
          pnodes.map(pnode => {
            pnode.createdOn = new Date(pnode.createdOn); // on reception date is a string
            return pnode;
          })
        )
      );
  }
}
