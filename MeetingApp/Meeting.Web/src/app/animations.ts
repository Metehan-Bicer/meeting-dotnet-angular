import { trigger, state, style, transition, animate, group, query } from '@angular/animations';

export const flyInOut = trigger('flyInOut', [
  state('in', style({ transform: 'translateX(0)' })),
  transition('void => *', [
    style({ transform: 'translateX(-100%)' }),
    animate(250)
  ]),
  transition('* => void', [
    animate(250, style({ transform: 'translateX(100%)' }))
  ])
]);

export const expand = trigger('expand', [
  transition('void => *', [
    style({ opacity: 0, transform: 'scale(0.8)' }),
    animate('300ms ease-in', style({ opacity: 1, transform: 'scale(1)' }))
  ])
]);

export const fadeInOut = trigger('fadeInOut', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('300ms ease-in', style({ opacity: 1 }))
  ]),
  transition(':leave', [
    animate('300ms ease-out', style({ opacity: 0 }))
  ])
]);

export const slideInAnimation = trigger('routeAnimations', [
  transition('* <=> *', [
    style({ position: 'relative' }),
    group([
      query(':enter, :leave', [
        style({
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%'
        })
      ], { optional: true }),
      query(':enter', [
        style({ opacity: 0 })
      ], { optional: true }),
      query(':leave', [
        animate('300ms ease-out', style({ opacity: 0 }))
      ], { optional: true }),
      query(':enter', [
        animate('300ms ease-in', style({ opacity: 1 }))
      ], { optional: true })
    ])
  ])
]);