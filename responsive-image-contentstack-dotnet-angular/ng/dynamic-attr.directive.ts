// /libs/shared/src/lib/directives/image-helper.directive.ts
import {
  Directive,
  ElementRef,
  effect,
  inject,
  input,
  Renderer2,
} from '@angular/core';
import { AppLoggerService } from '@website/core';

/*
 * Custom image optimization directive for handling srcsets and custom CDN support
 */

interface ISize
{
  w: number;
  h?: number;
}

const DEFAULT_IMAGE_DIMENSIONS: ISize[] = [
  { w: 320 } /* mobile */,
  { w: 768 } /* tablet portrait */,
  { w: 1024 } /* tablet landscape */,
  { w: 1280 } /* laptop */,
  { w: 1760 } /* container max width */,

  /* 2x retina */
  { w: 640 } /* also phablet */,
  { w: 1536 } /* also widescreen monitors */,
  { w: 2048 },
  { w: 2560 },
  { w: 3520 },

  /* 3x retina */
  { w: 960 },
  { w: 2304 },
  { w: 3072 },
  { w: 3840 },
  { w: 5280 },
];

@Directive({
  selector: '[imageHelper]',
  standalone: true,
})
export class ImageHelperDirective
{
  private readonly logger = inject(AppLoggerService).withScope(
    'ImageHelperDirective',
  );

  ngSrc = input.required<string>();

  /** list of widths (in pixels) used to generate the srcset */
  dimensions = input<ISize[]>(DEFAULT_IMAGE_DIMENSIONS);

  format = input<'webp | jpg | gif | png'>();

  aspectRatio = input<number>();

  private readonly el = inject(ElementRef);
  private readonly renderer = inject(Renderer2);

  constructor()
  {
    effect(() => this.update());
  }

  private update()
  {
    const src = this.ngSrc();
    const dimensions = [...(this.dimensions() || [])].sort((a, b) => a.w - b.w);
    const format = this.format() || 'webp';
    const ar = this.aspectRatio();

    this.logger.debug('Updating image props', src, dimensions, format);

    // build a simple srcset using either provided loader or default behavior
    const buildUrl = (s: string, w: number, h?: number, fmt?: string) =>
    {
      const base = s.split('?')[0];
      const computedHeight = h || (ar ? Math.round(w / ar) : undefined);
      const params = [
        `width=${w}`,
        ...(computedHeight ? [`height=${computedHeight}`, `fit=crop`] : []),
        `format=${fmt}`,
      ];
      return `${base}?${params.join('&')}`;
    };

    const computedSrcset = dimensions
      .map((size) => `${buildUrl(src, size.w, size.h, format)} ${size.w}w`)
      .join(', ');

    const defaultSrc = buildUrl(src, dimensions[0].w, dimensions[0].h, format);

    // Write props
    this.renderer.setAttribute(this.el.nativeElement, 'src', defaultSrc);
    //this.renderer.setAttribute(this.el.nativeElement, 'sizes', computedSizes);
    this.renderer.setAttribute(this.el.nativeElement, 'srcset', computedSrcset);
    this.renderer.setAttribute(this.el.nativeElement, 'decoding', 'async');
  }
}
