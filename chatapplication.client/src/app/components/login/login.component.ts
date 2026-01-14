import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { SignalRService } from '../../services/signalr.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = '';
  username = '';
  password = '';
  isLoginMode = true;
  errorMessage = '';
  isLoading = false;

  constructor(
    private authService: AuthService,
    private signalRService: SignalRService,
    private router: Router
  ) { }

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
  }

  onSubmit() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please fill in all fields';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    if (this.isLoginMode) {
      this.login();
    } else {
      this.register();
    }
  }

  private login() {
    this.authService.login(
      {
        email: this.email,
        password: this.password
      }
    ).subscribe(
      {
        next: (res) => {
          if (res && res.token) 
            {
              this.signalRService.startConnection(res.token);
              this.router.navigate(['/chat']);
            }
          else
            {
              this.errorMessage = 'Login failed: Invalid response.';
              this.isLoading = false;
            }

        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Login failed';
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      }
    );
  }

  private register() {
    this.authService.register({
      username: this.username,
      password: this.password,
      email: this.email
    }).subscribe({
      next: (res) => {
        this.signalRService.startConnection(res.token);
        this.router.navigate(['/chat']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Registration failed';
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
