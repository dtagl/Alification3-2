// import React from "react";
// import { useNavigate } from "react-router-dom";
// import { ArrowLeft } from "lucide-react";

// export const BackButton: React.FC<{ label?: string; className?: string }> = ({
//   label = "Назад",
//   className = "",
// }) => {
//   const nav = useNavigate();

//   return (
//     <button
//       onClick={() => nav(-1)}
//       className={`inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-gray-200/70 
//                  hover:bg-gray-300 hover:shadow-md active:scale-[0.97] 
//                  text-gray-800 font-medium transition-all duration-150 backdrop-blur-sm ${className}`}
//     >
//       <ArrowLeft className="w-5 h-5" />
//       {label}
//     </button>
//   );
// };

// export default BackButton;



import { useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";

export default function BackButton() {
  const nav = useNavigate();

  return (
    <button
      onClick={() => nav(-1)}
      className="flex items-center gap-2 px-3 py-2 rounded-lg border border-gray-300 bg-white text-gray-700 
                 hover:bg-gray-100 hover:border-gray-400 transition-all active:scale-[0.97] shadow-sm"
    >
      <ArrowLeft className="w-4 h-4" />
      <span>Назад</span>
    </button>
  );
}



// это я создал для кгопки 'Назад' 

