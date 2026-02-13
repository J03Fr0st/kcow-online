import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="form-modal">
      <form (ngSubmit)="onSubmit()">
        <div class="form-group">
          <label for="name">Name</label>
          <input
            type="text"
            id="name"
            class="form-control"
            [(ngModel)]="formData.name"
            name="name"
            placeholder="Enter your name"
            required
          />
        </div>

        <div class="form-group">
          <label for="email">Email</label>
          <input
            type="email"
            id="email"
            class="form-control"
            [(ngModel)]="formData.email"
            name="email"
            placeholder="Enter your email"
            required
          />
        </div>

        <div class="form-group">
          <label for="role">Role</label>
          <select
            id="role"
            class="form-control"
            [(ngModel)]="formData.role"
            name="role"
          >
            <option value="">Select a role</option>
            <option value="admin">Admin</option>
            <option value="user">User</option>
            <option value="guest">Guest</option>
          </select>
        </div>

        <div class="form-group">
          <label for="message">Message</label>
          <textarea
            id="message"
            class="form-control"
            [(ngModel)]="formData.message"
            name="message"
            rows="4"
            placeholder="Enter a message"
          ></textarea>
        </div>

        <div class="form-actions">
          <button type="button" class="btn btn-outline" (click)="onCancel()">
            Cancel
          </button>
          <button
            type="submit"
            class="btn btn-primary"
            [disabled]="!isValid()"
          >
            Submit
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [
    `
    .form-modal {
      padding: 0.5rem 0;
    }

    .form-group {
      margin-bottom: 1.25rem;

      label {
        display: block;
        font-size: 0.875rem;
        font-weight: 500;
        color: var(--text-color);
        margin-bottom: 0.5rem;
      }

      .form-control {
        width: 100%;
        padding: 0.5rem 0.75rem;
        font-size: 0.875rem;
        border: 1px solid var(--border-color);
        border-radius: 4px;
        background-color: var(--input-background, white);
        color: var(--text-color);
        transition: border-color 0.2s;

        &:focus {
          outline: none;
          border-color: var(--primary-color);
          box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

        &::placeholder {
          color: var(--text-muted);
        }
      }

      textarea.form-control {
        resize: vertical;
        min-height: 80px;
      }

      select.form-control {
        cursor: pointer;
      }
    }

    .form-actions {
      display: flex;
      gap: 0.75rem;
      justify-content: flex-end;
      margin-top: 1.5rem;
      padding-top: 1.5rem;
      border-top: 1px solid var(--border-color);
    }

    .btn {
      padding: 0.5rem 1.5rem;
      border-radius: 4px;
      border: none;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;

      &:focus {
        outline: 2px solid var(--primary-color);
        outline-offset: 2px;
      }

      &:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }

      &.btn-primary {
        background-color: var(--primary-color);
        color: white;

        &:hover:not(:disabled) {
          background-color: var(--primary-hover);
        }
      }

      &.btn-outline {
        background-color: transparent;
        color: var(--text-color);
        border: 1px solid var(--border-color);

        &:hover {
          background-color: var(--hover-background);
        }
      }
    }
  `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormModalComponent {
  @Output() closeModal = new EventEmitter<unknown>();
  @Output() dismissModal = new EventEmitter<unknown>();

  formData = {
    name: '',
    email: '',
    role: '',
    message: '',
  };

  isValid(): boolean {
    return !!(this.formData.name && this.formData.email);
  }

  onSubmit(): void {
    if (this.isValid()) {
      this.closeModal.emit(this.formData);
    }
  }

  onCancel(): void {
    this.dismissModal.emit('User cancelled');
  }
}
