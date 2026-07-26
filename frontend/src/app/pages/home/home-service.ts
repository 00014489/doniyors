import { Injectable, signal } from '@angular/core';
import { User } from '../../core/models/user';
import { Adventure } from '../../core/models/adventure';

@Injectable({
  providedIn: 'root',
})
export class HomeService {
  readonly users = signal<User[]>([
    {
      id: 1,
      username: 'Alex Johnson',
      avatar: 'https://i.pravatar.cc/150?img=1',
      points: 1250,
    },
    {
      id: 2,
      username: 'Emma Wilson',
      avatar: 'https://i.pravatar.cc/150?img=5',
      points: 980,
    },
    {
      id: 3,
      username: 'Liam Brown',
      avatar: 'https://i.pravatar.cc/150?img=12',
      points: 1675,
    },
    {
      id: 4,
      username: 'Sophia Davis',
      avatar: 'https://i.pravatar.cc/150?img=20',
      points: 1430,
    },
    {
      id: 5,
      username: 'Noah Miller',
      avatar: 'https://i.pravatar.cc/150?img=30',
      points: 870,
    },
  ]);

  readonly adventures = signal<Adventure[]>([
    {
      id: 1,
      title: 'Mountain Sunrise Hike',
      image: 'https://picsum.photos/600/400?random=1',
      date: '2026-08-10',
      location: 'Rocky Mountains',
      reward: 250,
      description:
        'Enjoy a scenic sunrise hike through breathtaking mountain trails.',
      difficulty: 'Medium',
      duration: '4 hours',
      participants: 12,
      maxParticipants: 20,
      equipment: 'Hiking boots, backpack, water bottle',
    },
    {
      id: 2,
      title: 'Forest Survival Challenge',
      image: 'https://picsum.photos/600/400?random=2',
      date: '2026-08-18',
      location: 'Greenwood Forest',
      reward: 500,
      description:
        'Learn essential survival skills in a guided wilderness adventure.',
      difficulty: 'Hard',
      duration: '8 hours',
      participants: 8,
      maxParticipants: 15,
      equipment: 'Knife, compass, flashlight, camping gear',
    },
    {
      id: 3,
      title: 'Lakeside Kayaking',
      image: 'https://picsum.photos/600/400?random=3',
      date: '2026-08-22',
      location: 'Crystal Lake',
      reward: 180,
      description:
        'Paddle through calm waters while enjoying beautiful lake scenery.',
      difficulty: 'Easy',
      duration: '2.5 hours',
      participants: 10,
      maxParticipants: 16,
      equipment: 'Life jacket, kayak, paddle',
    },
    {
      id: 4,
      title: 'Desert ATV Expedition',
      image: 'https://picsum.photos/600/400?random=4',
      date: '2026-09-05',
      location: 'Red Sand Dunes',
      reward: 400,
      description:
        'Experience an adrenaline-filled ATV ride across stunning desert landscapes.',
      difficulty: 'Medium',
      duration: '5 hours',
      participants: 6,
      maxParticipants: 10,
      equipment: 'Helmet, gloves, sunglasses',
    },
    {
      id: 5,
      title: 'Waterfall Trek',
      image: 'https://picsum.photos/600/400?random=5',
      date: '2026-09-12',
      location: 'Silver Falls',
      reward: 300,
      description:
        'Trek through lush trails to discover hidden waterfalls and natural pools.',
      difficulty: 'Easy',
      duration: '3 hours',
      participants: 14,
      maxParticipants: 25,
      equipment: 'Hiking shoes, water bottle, camera',
    },
  ]);
}
