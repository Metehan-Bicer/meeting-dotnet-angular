import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dateFormat'
})
export class DateFormatPipe implements PipeTransform {
  transform(value: any, format: string = 'short'): any {
    if (!value) return '';
    
    const date = new Date(value);
    if (isNaN(date.getTime())) return value;
    
    switch (format) {
      case 'short':
        return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      case 'medium':
        return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
      case 'long':
        return date.toLocaleString();
      case 'dateOnly':
        return date.toLocaleDateString();
      case 'timeOnly':
        return date.toLocaleTimeString();
      default:
        return date.toLocaleString();
    }
  }
}