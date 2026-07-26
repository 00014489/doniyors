export interface Adventure {

  id: number;

  title: string;

  image: string;

  date: string;

  location: string;

  reward: number;

  description: string;

  difficulty: 'Easy' | 'Medium' | 'Hard';

  duration: string;

  participants: number;

  maxParticipants: number;

  equipment: string;
}