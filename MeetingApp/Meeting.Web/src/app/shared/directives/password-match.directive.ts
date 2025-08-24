import { Directive, Input } from '@angular/core';
import { NG_VALIDATORS, Validator, AbstractControl, ValidationErrors } from '@angular/forms';

@Directive({
  selector: '[appPasswordMatch]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: PasswordMatchDirective,
      multi: true
    }
  ]
})
export class PasswordMatchDirective implements Validator {
  @Input('appPasswordMatch') matchTo: string = '';

  validate(control: AbstractControl): ValidationErrors | null {
    if (!control.parent) {
      return null;
    }

    const controlToMatch = control.parent.get(this.matchTo);
    if (!controlToMatch) {
      return null;
    }

    if (controlToMatch.value !== control.value) {
      return { passwordMismatch: true };
    }

    return null;
  }
}