import { HttpClient } from '@angular/common/http';
import { Component, OnInit, NgModule } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
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
    this.person = null;
    this.personDuties = null;
    this.loading = true;

    this.clearMessages();

    if (input.value.length == 0) {
      this.errorMessage = 'Can not search for blank names. Please enter name and try again.';
      this.loading = false;
      return;
    }

    const searchValue = input.value;
    input.value = '';
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
        // this sometimes eats stuff I would rather it not so setup in the interface "OK for endpoint"
        this.loading = false;
        console.error('Error: ', error);
        this.errorMessage = error.statusText;
      }
    });
  }
}

interface ApiResponse {
  success?: boolean;
  message: string;
  [key: string]: any;
}
