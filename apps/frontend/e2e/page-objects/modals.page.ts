import { type Page, type Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page object for Modal/Dialog functionality
 */
export class ModalsPage extends BasePage {
  readonly openModalButton: Locator;
  readonly openConfirmButton: Locator;
  readonly openAlertButton: Locator;
  readonly openNestedButton: Locator;
  readonly modal: Locator;
  readonly modalTitle: Locator;
  readonly modalCloseButton: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);

    this.openModalButton = page.locator('button', { hasText: /open modal/i }).first();
    this.openConfirmButton = page.locator('button', { hasText: /confirm/i }).first();
    this.openAlertButton = page.locator('button', { hasText: /alert/i }).first();
    this.openNestedButton = page.locator('button', { hasText: /nested/i });
    this.modal = page.locator('.modal.modal-open');
    this.modalTitle = this.modal.locator('h3, .modal-title').first();
    this.modalCloseButton = this.modal.locator('button', { hasText: /close/i });
    this.confirmButton = this.modal.locator('button', { hasText: /confirm|ok|yes/i });
    this.cancelButton = this.modal.locator('button', { hasText: /cancel|no/i });
  }

  async gotoModals(): Promise<void> {
    await this.goto('/modals');
  }

  async openModal(): Promise<void> {
    await this.openModalButton.click();
    await this.helpers.waitForModal();
  }

  async openConfirmDialog(): Promise<void> {
    await this.openConfirmButton.click();
    await this.helpers.waitForModal();
  }

  async openAlertDialog(): Promise<void> {
    await this.openAlertButton.click();
    await this.helpers.waitForModal();
  }

  async closeModal(): Promise<void> {
    await this.modalCloseButton.click();
    await this.helpers.waitForModalToClose();
  }

  async confirmDialog(): Promise<void> {
    await this.confirmButton.click();
    await this.helpers.waitForModalToClose();
  }

  async cancelDialog(): Promise<void> {
    await this.cancelButton.click();
    await this.helpers.waitForModalToClose();
  }

  async isModalOpen(): Promise<boolean> {
    return await this.modal.isVisible();
  }

  async getModalCount(): Promise<number> {
    return await this.page.locator('.modal.modal-open').count();
  }

  async getModalSize(): Promise<string | null> {
    const classList = await this.modal.getAttribute('class');
    if (!classList) return null;

    const sizeClasses = ['modal-sm', 'modal-md', 'modal-lg', 'modal-xl', 'modal-full'];
    for (const sizeClass of sizeClasses) {
      if (classList.includes(sizeClass)) {
        return sizeClass.replace('modal-', '');
      }
    }
    return 'default';
  }
}
