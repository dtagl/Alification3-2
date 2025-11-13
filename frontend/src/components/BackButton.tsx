import React from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";

export const BackButton: React.FC<{ label?: string; className?: string }> = ({
  label = "Назад",
  className = "",
}) => {
  const nav = useNavigate();

  return (
    <button
      onClick={() => nav(-1)}
      className={`inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-gray-200/70 
                 hover:bg-gray-300 hover:shadow-md active:scale-[0.97] 
                 text-gray-800 font-medium transition-all duration-150 backdrop-blur-sm ${className}`}
    >
      <ArrowLeft className="w-5 h-5" />
      {label}
    </button>
  );
};

export default BackButton;
