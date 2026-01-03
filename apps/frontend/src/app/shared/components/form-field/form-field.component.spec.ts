import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormFieldComponent } from './form-field.component';
import { Component } from '@angular/core';

@Component({
  template: `
    <app-form-field [label]="label" [inputId]="inputId" [error]="error" [hint]="hint" [required]="required">
      <input [id]="inputId" type="text" class="input input-bordered w-full" />
    </app-form-field>
  `,
  standalone: true,
  imports: [FormFieldComponent],
})
class TestHostComponent {
  label = 'Test Label';
  inputId = 'test-input';
  error = '';
  hint = '';
  required = false;
}

describe('FormFieldComponent', () => {
  let component: TestHostComponent;
  let fixture: ComponentFixture<TestHostComponent>;
  let compiled: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormFieldComponent, TestHostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should render label with correct text', () => {
    fixture.detectChanges();
    const label = compiled.querySelector('label.label');
    expect(label).toBeTruthy();
    expect(label?.textContent).toContain('Test Label');
  });

  it('should associate label with input via for attribute', () => {
    fixture.detectChanges();
    const label = compiled.querySelector('label.label') as HTMLLabelElement;
    expect(label?.getAttribute('for')).toBe('test-input');
  });

  it('should display error message when error is provided', async () => {
    component.error = 'This field is required';
    fixture.detectChanges();
    await fixture.whenStable();

    const errorSpan = compiled.querySelector('[role="alert"]');
    expect(errorSpan).toBeTruthy();
    expect(errorSpan?.textContent?.trim()).toBe('This field is required');
  });

  it('should add error id for aria-describedby linking', async () => {
    component.error = 'Invalid value';
    fixture.detectChanges();
    await fixture.whenStable();

    const errorSpan = compiled.querySelector('[role="alert"]');
    expect(errorSpan?.getAttribute('id')).toBe('test-input-error');
  });

  it('should display hint text when provided and no error', () => {
    component.hint = 'Enter your email address';
    fixture.detectChanges();
    fixture.detectChanges(); // Second detectChanges to handle OnPush

    const hintSpan = compiled.querySelector('.label-text-alt');
    expect(hintSpan).toBeTruthy();
    expect(hintSpan?.textContent?.trim()).toBe('Enter your email address');
  });

  it('should not display hint when error is present', () => {
    component.error = 'Error message';
    component.hint = 'Hint message';
    fixture.detectChanges();
    fixture.detectChanges(); // Second detectChanges to handle OnPush

    const hintSpan = compiled.querySelector('#test-input-hint');
    expect(hintSpan).toBeFalsy();
  });

  it('should display required asterisk when required is true', () => {
    component.required = true;
    fixture.detectChanges();
    fixture.detectChanges(); // Second detectChanges to handle OnPush

    const requiredSpan = compiled.querySelector('[aria-label="required"]');
    expect(requiredSpan).toBeTruthy();
    expect(requiredSpan?.textContent?.trim()).toBe('*');
  });

  it('should not display required asterisk when required is false', () => {
    component.required = false;
    fixture.detectChanges();

    const requiredSpan = compiled.querySelector('[aria-label="required"]');
    expect(requiredSpan).toBeFalsy();
  });

  it('should apply correct DaisyUI classes', () => {
    fixture.detectChanges();
    const formControl = compiled.querySelector('.form-control');
    expect(formControl).toBeTruthy();

    const label = compiled.querySelector('.label');
    expect(label).toBeTruthy();

    const labelText = compiled.querySelector('.label-text');
    expect(labelText).toBeTruthy();
  });

  it('should render projected content (input)', () => {
    fixture.detectChanges();
    const input = compiled.querySelector('input#test-input');
    expect(input).toBeTruthy();
  });
});
