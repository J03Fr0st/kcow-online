import type { Type } from '@angular/core';

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';

export interface ModalConfig {
  title?: string;
  size?: ModalSize;
  dismissible?: boolean;
  backdropDismiss?: boolean;
  showCloseButton?: boolean;
  data?: unknown;
}

export interface ModalButton {
  label: string;
  class?: string;
  action: () => void | Promise<void>;
}

export interface ConfirmationConfig {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmClass?: string;
  cancelClass?: string;
  size?: ModalSize;
}

export interface Modal {
  id: string;
  component?: Type<unknown>;
  config: ModalConfig;
  resolve: (result?: unknown) => void;
  reject: (reason?: unknown) => void;
}
