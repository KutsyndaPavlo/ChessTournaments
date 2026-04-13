import { ToastMessageOptions } from 'primeng/api';

export const SUCCESS_TOAST: ToastMessageOptions = {
  closable: true,
  severity: 'success',
  life: 3000,
  contentStyleClass: 'items-center *:gap-0',
};

export const ERROR_TOAST: ToastMessageOptions = {
  closable: true,
  severity: 'error',
  life: 5000,
  contentStyleClass: 'items-center *:gap-0',
};

export const WARNING_TOAST: ToastMessageOptions = {
  closable: true,
  severity: 'warn',
  life: 4000,
  contentStyleClass: 'items-center *:gap-0',
};

export const INFO_TOAST: ToastMessageOptions = {
  closable: true,
  severity: 'info',
  life: 3000,
  contentStyleClass: 'items-center *:gap-0',
};

export const CONFIRM_UNSAVED_CHANGES_MESSAGE = {
  header: 'Unsaved Changes',
  message: 'You have unsaved changes. Are you sure you want to leave this page?',
  icon: 'pi pi-exclamation-triangle',
  acceptLabel: 'Leave',
  rejectLabel: 'Stay',
  acceptButtonStyleClass: 'p-button-danger',
  rejectButtonStyleClass: 'p-button-text',
};
