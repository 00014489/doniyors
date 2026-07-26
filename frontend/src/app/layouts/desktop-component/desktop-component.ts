import { Component } from '@angular/core';
import { DeskNav } from "../../shared/components/desk-nav/desk-nav";
import { RouterOutlet } from "@angular/router";

@Component({
  selector: 'app-desktop-component',
  imports: [DeskNav, RouterOutlet],
  templateUrl: './desktop-component.html',
  styleUrl: './desktop-component.scss',
})
export class DesktopComponent {
  
}
