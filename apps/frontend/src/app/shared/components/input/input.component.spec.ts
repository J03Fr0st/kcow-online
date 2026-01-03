import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InputComponent } from './input.component';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  template: `
    <app-input
      [inputId]="inputId"
      [type]="type"
      [placeholder]="placeholder"
      [hasError]="hasError"
      [required]="required"
      [(ngModel)]="value">
    </app-input>
  `,
  standalone: true,
  imports: [InputComponent, FormsModule],
})
class TestHostComponent {
  inputId = 'test-input';
  type: 'text' | 'email' | 'password' = 'text';
  placeholder = 'Enter value';
  hasError = false;
  required = false;
  value = '';
}

describe('InputComponent', () => {
  let component: TestHostComponent;
  let fixture: ComponentFixture<TestHostComponent>;
  let compiled: HTMLElement;
  let inputElement: HTMLInputElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputComponent, TestHostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement;
    inputElement = compiled.querySelector('input') as HTMLInputElement;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should render input with correct id', () => {
    fixture.detectChanges();
    expect(inputElement).toBeTruthy();
    expect(inputElement.getAttribute('id')).toBe('test-input');
  });

  it('should apply correct type attribute', () => {
    component.type = 'email';
    fixture.detectChanges();
    expect(inputElement.getAttribute('type')).toBe('email');
  });

  it('should apply placeholder text', () => {
    fixture.detectChanges();
    expect(inputElement.getAttribute('placeholder')).toBe('Enter value');
  });

  it('should add aria-invalid when hasError is true', () => {
    component.hasError = true;
    fixture.detectChanges();

    expect(inputElement.getAttribute('aria-invalid')).toBe('true');
  });

  it('should not have aria-invalid when hasError is false', () => {
    component.hasError = false;
    fixture.detectChanges();

    expect(inputElement.getAttribute('aria-invalid')).toBeNull();
  });

  it('should add aria-describedby when hasError is true', () => {
    component.hasError = true;
    fixture.detectChanges();

    expect(inputElement.getAttribute('aria-describedby')).toBe('test-input-error');
  });

  it('should not have aria-describedby when hasError is false', () => {
    component.hasError = false;
    fixture.detectChanges();

    expect(inputElement.getAttribute('aria-describedby')).toBeNull();
  });

  it('should apply DaisyUI input classes', () => {
    fixture.detectChanges();
    expect(inputElement.classList.contains('input')).toBe(true);
    expect(inputElement.classList.contains('input-bordered')).toBe(true);
  });

  it('should add input-error class when hasError is true', () => {
    component.hasError = true;
    fixture.detectChanges();

    expect(inputElement.classList.contains('input-error')).toBe(true);
  });

  it('should not have input-error class when hasError is false', () => {
    component.hasError = false;
    fixture.detectChanges();

    expect(inputElement.classList.contains('input-error')).toBe(false);
  });

  it('should handle ngModel binding', async () => {
    component.value = 'test value';
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(inputElement.value).toBe('test value');
  });

  it('should update ngModel on input', async () => {
    fixture.detectChanges();
    inputElement.value = 'new value';
    inputElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.value).toBe('new value');
  });

  it('should set required attribute when required is true', () => {
    component.required = true;
    fixture.detectChanges();

    expect(inputElement.hasAttribute('required')).toBe(true);
  });

  it('should handle disabled state', () => {
    fixture.detectChanges();
    const inputComponent = fixture.debugElement.query((el) => el.componentInstance instanceof InputComponent).componentInstance as InputComponent;

    inputComponent.setDisabledState(true);
    fixture.detectChanges();
    fixture.detectChanges();

    const input = compiled.querySelector('input') as HTMLInputElement;
    expect(input.disabled).toBe(true);
  });
});
