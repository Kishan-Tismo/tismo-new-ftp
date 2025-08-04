import { Component, OnInit } from '@angular/core';
import {DataService} from "../Dataservice/dataservice.service";

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss']
})
export class MainComponent   {


  private static localStorageKey = 'currentUserData';
  static stShowLogin = true

  get showLogin()  { return MainComponent.stShowLogin }

  title = "Tismo File Transfer"
  userName = ""
  password = ""

  pwdinputType = "password" // toggles between password and text. password type => password typed is hidden

  messageTimeout = 2500
  build = "Version 2.4, 7 Aug 2024"


  onPwdVisibleClick() {
      this.pwdinputType = this.pwdinputType == "password" ? "text" : "password"
  }

  private _successMessage = ""
  get successMessage(): string { return this._successMessage;}
  set successMessage(value: string) { // auto reset
    this._successMessage = value;
    if (value != "") {
      this._successMessage = ""
      setTimeout(() => this._successMessage = "", this.messageTimeout) // clear  afterwards
    }
  }

  private _errorMessage = ""
  customerCode: string = ""
  get errorMessage() { return this._errorMessage }
  set errorMessage(val) { // auto reset
    this._errorMessage = val
    if (val != "") {
      this._successMessage = "" // clear normal message when error message is active
      setTimeout(() => this._errorMessage = "", this.messageTimeout) // clear  afterwards
    }
  }


  getLocalToken() {
    return localStorage.getItem(MainComponent.localStorageKey)
  }
  static clearToken()
  {
    localStorage.setItem(MainComponent.localStorageKey, "")
  }
  setToken(token:string) {
    localStorage.setItem(MainComponent.localStorageKey,token)
  }

  static LoggedIn() {

    MainComponent.stShowLogin = false
  }

  static LogOut() {
      MainComponent.clearToken();
      DataService.authString = ""
      MainComponent.stShowLogin = true

  }

  constructor(private _data: DataService) {
    let token : string | null = this.getLocalToken()
    if (token != null ) {
      _data.setHttpAuthorizationHeader(token)
      _data.GetFileList("/")
        .subscribe({
          next: t => {
            MainComponent.LoggedIn()
            _data.GetCustomerCode()
              .subscribe(ccode => this.customerCode = ccode)
          },
          error: err => MainComponent.LogOut(),
          complete: () => {

            MainComponent.LoggedIn()
          }
        })
    }
    else {
      MainComponent.LogOut()
    }
  }

  onClick() {
    console.log('logging in')
    this.successMessage = ''
    this.errorMessage = ''

    this._data.Login( this.userName, this.password).subscribe(
      {
        next: token => {
          if (token) {
            this.successMessage = 'Logged In'
            this.errorMessage = ''
            this._data.setHttpAuthorizationHeader(token)
            this.setToken(token)
            this.password = "" // blank out the password after loggin
            MainComponent.LoggedIn()

          } else {
            this.successMessage = ''
            this.errorMessage = 'Login failure'
            MainComponent.LogOut()
          }
        },
       error: err => {
          this.successMessage = ''
          this.errorMessage = 'Login failure'
          MainComponent.LogOut()
          console.log(err)
        }
     }
    );
  }


}
