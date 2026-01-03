import type { Type } from '@angular/core';

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';

export interface ModalConfig {
  title?: string;
  size?: ModalSize;
  dismissible?: boolean;
  backdropDismiss?: boolean;
  showCloseButton?: boolean;
  data?: any;
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
  component?: Type<any>;
  config: ModalConfig;
  resolve: (result?: any) => void;
  reject: (reason?: any) => void;
}
