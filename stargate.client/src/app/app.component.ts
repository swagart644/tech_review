import { HttpClient } from '@angular/common/http';
import { Component, OnInit, NgModule } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: [ './app.component.css' ]
})
export class AppComponent {
  message: string = '';
  errorMessage: string = '';
  loading: boolean = false;
  person: any = null;
  personDuties: any = null;
  constructor(private http: HttpClient) { }

  public clearMessages(): void {
    this.message = '';
    this.errorMessage = '';
  }

  public clearPeople(): void {
    this.person = null;
    this.personDuties = null;
    this.clearMessages();
  }

  public search(input: HTMLInputElement): void {
    const searchValue = input.value.trim();
    input.value = '';

    if (searchValue.length == 0) {
      this.errorMessage = 'Can not search for blank names. Please enter name and try again.';
      return;
    }

    if (searchValue === (this.person?.name ?? '')) {
      console.log('Stopped repeat search for: ' + searchValue);
      return;
    }

    this.clearPeople();
    this.loading = true;
    
    const apiUrl = '/astronautduty/' + searchValue;

    this.http.get<ApiResponse>(apiUrl).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success) {
          if (response['person'] === null) {
            this.errorMessage = 'Could not find user ' + searchValue;
          } else {
            this.message = 'Successfully found user ' + searchValue;
            this.person = response['person'];
            this.personDuties = response['astronautDuties'];
          }
        } else {
          this.errorMessage = response.message;
        }
      },
      error: (error) => {
        console.error('Error: ', error);
        if (error.error.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = error.statusText;
        }
        this.loading = false;
      }
    });
  }
}

interface ApiResponse {
  success?: boolean;
  message: string;
  [key: string]: any;
}
